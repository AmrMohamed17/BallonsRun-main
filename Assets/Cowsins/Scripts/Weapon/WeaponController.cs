using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace cowsins2D
{

    public class WeaponController : MonoBehaviour
    {
        #region variables

        [System.Serializable]
        public class CustomShotMethods
        {
            public Weapon_SO weapon;
            public UnityEvent OnShoot, OnShooting, OnNotShooting;
        }

        [SerializeField, Tooltip("Reference for the player camera.")] private Camera playerCamera;

        [SerializeField, Tooltip("Sensitivty for the aim when using a controller.")] private float controllerAimSensitivity;

        [Tooltip("Transform that contains all the weapons during the game.")] public Transform weaponHolder;

        [SerializeField, Tooltip("Assign the Weapon_SO of your initial weapon. Do not assign more than the hotbar size.")] private Weapon_SO[] initialWeapons;

        [SerializeField, Tooltip("Assign all the layers that can be hit by the player.")] private LayerMask hitLayer;

        private Dictionary<int, int> sortedHitLayer = new Dictionary<int, int>();

        public delegate void Aim();

        public event Aim aim;

        public Vector2 aimingOrientation { get; private set; }

        public delegate void Shoot();

        public event Shoot shoot;

        private UnityEvent onCustomShoot, onCustomShooting, onCustomNotShooting;

        [Tooltip("Used for weapons with custom shot method. Here, " +
            "you can attach your scriptable objects and assign the method you want to call on shoot. " +
            "Please only assign those scriptable objects that use custom shot methods, Otherwise it won�t work or you will run into issues.")]
        public CustomShotMethods[] customShot;

        [Tooltip("Allow shooting while in a ladder.")] public bool canShootWhileLadder;

        [Tooltip("Allow shooting when gliding.")] public bool canShootWhileGliding;

        // This variable detects if the player can shoot, mainly based on the fire rate of the weapon.
        // At start, you can shoot, so the variable is true.
        public bool canShoot { get; private set; } = true;

        public Weapon_SO weapon;

        public delegate void Reload();

        public event Reload reload;

        public bool reloading { get; private set; } = false;


        [HideInInspector] public int currentWeapon;

        public float heatAmount { get; private set; }

        public WeaponIdentification id { get; private set; }

        public WeaponIdentification[] inventory { get; private set; }

        public bool weaponHidden { get; private set; } = false;

        public Camera PlayerCamera
        {
            get { return playerCamera; }
        }

        private PlayerMovement player;

        private PlayerStats playerStats;

        private InteractionManager interactManager;

        [System.Serializable]
        public class Events
        {
            public UnityEvent onShoot, onReload, onUnholster;
        }

        public Events events;



        #endregion

        #region main

        private void OnEnable()
        {
            UIController.onSlotPointerUp += WeaponInvReleaseCheck;
        }
        private void OnDisable()
        {
            UIController.onSlotPointerUp -= WeaponInvReleaseCheck;
        }

        private void Start()
        {
            currentWeapon = 0;
            interactManager = GetComponent<InteractionManager>();
            inventory = new WeaponIdentification[interactManager.hotbarSize];
            player = GetComponent<PlayerMovement>();
            playerStats = GetComponent<PlayerStats>();
            GetInitialWeapons();
            SortHitLayer();
        }
        private void Update()
        {
            if (weaponHolder.localPosition != Vector3.zero) weaponHolder.localPosition = Vector3.Lerp(weaponHolder.localPosition, Vector3.zero, Time.deltaTime * (weapon == null ? 3 : weapon.visualRecoilRecovery));

            if (!PlayerStats.controllable || playerStats.isDead) return;

            aim?.Invoke();

            if (weapon == null || weaponHidden) return;

            if (PlayerIsShooting() && UIController.highlightedInventorySlot == null && UIController.currentInventorySlot == null) HandleShooting();
            else if (weapon.shootingStyle == Weapon_SO.ShootingStyle.Custom) onCustomNotShooting?.Invoke();

            reload?.Invoke();
        }
        #endregion


        #region shooting
        private void HandleShooting()
        {
            if (playerStats.isDead) return;
            if (weapon.shootingStyle == Weapon_SO.ShootingStyle.Custom) onCustomShooting?.Invoke();

            shoot?.Invoke();

            events.onShoot?.Invoke();

            // Reduce the amount of bullets except for melee weapons, since these do not use ammo
            if (weapon.shootingStyle != Weapon_SO.ShootingStyle.Melee && !weapon.infiniteBullets) id.currentBullets -= weapon.ammoPerShot;

            // Reset the auxiliar variable
            // This variable is only used for ReleaseWhenReady Shooting Method.
            canReleaseAndShoot = false;

            // Reset the shot when the fire rate allows.
            // This is used for all kinds of shooting styles and shooting methods.
            canShoot = false;
            Invoke(nameof(ResetShot), weapon.fireRate);

            // Play effects
            // Camera Shake
            if (weapon.camShake && CameraShake.Instance != null) CameraShake.Instance.Shake(weapon.camShakeAmount, 16, .8f, 17);

            UIController.updateWeaponInfo?.Invoke();
            id.currentBullets = id.currentBullets;
            id.totalBullets = id.totalBullets;
        }

        private void RaycastShot() => StartCoroutine(PerformRaycastShot());

        private void ProjectileShot() => StartCoroutine(PerformProjectileShot());

        private void CustomShot() => StartCoroutine(PerformCustomShot());

        private IEnumerator PerformRaycastShot()
        {
            if (weapon == null) yield break;
            for (int shots = 0; shots < weapon.bulletsPerShot; shots++)
            {
                Vector2 spread = new Vector2(Random.Range(-weapon.spread, weapon.spread), Random.Range(-weapon.spread, weapon.spread));
                RaycastHit2D hit = Physics2D.Raycast(transform.position, aimingOrientation + spread, 100, hitLayer);
                if (hit) Hit(hit.collider, hit.point, hit.normal);

                for (int i = 0; i < weapon.weaponObject.muzzles.Length; i++)
                {
                    Quaternion muzzleRotation = Quaternion.Euler(0, 0, Mathf.Atan2(aimingOrientation.y, aimingOrientation.x) * Mathf.Rad2Deg);
                    var wp = Instantiate(weapon.muzzleFlashVFX, id.muzzles[i].position, muzzleRotation);
                    wp.transform.parent = id.muzzles[i].transform;
                    if (i == 0)
                    {
                        wp.GetComponent<AudioSource>().clip = weapon.sounds.shotSFX;
                        wp.GetComponent<AudioSource>().Play();
                    }
                }
                Crosshair.Instance.Resize(weapon.crosshairShootingSpread);

                VisualRecoil();

                yield return new WaitForSeconds(weapon.timeBetweenBullets);
            }
            yield break;
        }

        private IEnumerator PerformProjectileShot()
        {
            int shots = 0;
            while (shots < weapon.bulletsPerShot)
            {
                Vector2 spread = new Vector2(Random.Range(-weapon.spread, weapon.spread), Random.Range(-weapon.spread, weapon.spread));

                for (int i = 0; i < weapon.weaponObject.muzzles.Length; i++)
                {
                    Quaternion muzzleRotation = Quaternion.Euler(0, 0, Mathf.Atan2(aimingOrientation.y, aimingOrientation.x) * Mathf.Rad2Deg);
                    Projectile proj = Instantiate(weapon.projectile, id.muzzles[i].position, Quaternion.identity);
                    proj.SetInitialSettings((aimingOrientation + spread).normalized, weapon.projectileSpeed, hitLayer, weapon.projectileCollisionSize, weapon.damage * PlayerMultipliers.Instance.damageModifier, weapon.explosiveProjectile, weapon.explosiveDamage * PlayerMultipliers.Instance.damageModifier, weapon.explosiveForce, weapon.explosionRadius);
                    proj.transform.GetChild(0).right = aimingOrientation + spread;
                    proj.projectileHit = Hit;
                    proj.Invoke(nameof(proj.DestroyProjectile), weapon.projectileDuration);
                    var flash = Instantiate(weapon.muzzleFlashVFX, id.muzzles[i].position, id.muzzles[i].rotation);
                    flash.transform.parent = id.muzzles[i].transform;
                    if (i == 0)
                    {
                        flash.GetComponent<AudioSource>().clip = weapon.sounds.shotSFX;
                        flash.GetComponent<AudioSource>().Play();
                    }
                }

                shots++;

                Crosshair.Instance.Resize(weapon.crosshairShootingSpread);

                VisualRecoil();

                yield return new WaitForSeconds(weapon.timeBetweenBullets);
            }
            yield break;
        }
        private void MeleeShot()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, weapon.meleeAttackRadius, hitLayer);

            SoundManager.Instance.PlaySound(weapon.sounds.shotSFX, 1);

            foreach (Collider2D collider in colliders)
            {
                // Handle the hit for each collider
                Hit(collider, collider.transform.position, Vector2.zero);
                if (collider.transform.CompareTag("Projectile") && weapon.canParry)
                {

                    // Get the angle
                    float angle = Mathf.Atan2(aimingOrientation.y, aimingOrientation.x) * Mathf.Rad2Deg;

                    // Rotate the projectile to face the aiming direction
                    collider.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                    collider.GetComponent<Rigidbody2D>().velocity = aimingOrientation * weapon.parryProjectileSpeed;
                }
            }

            if (id.GetComponentInChildren<Animator>()) id.GetComponentInChildren<Animator>().SetTrigger("Fire");
            VisualRecoil();
        }

        private IEnumerator PerformCustomShot()
        {
            int shots = 0;
            while (shots < weapon.bulletsPerShot)
            {
                onCustomShoot?.Invoke();

                for (int i = 0; i < weapon.weaponObject.muzzles.Length; i++)
                {
                    var flash = Instantiate(weapon.muzzleFlashVFX, id.muzzles[i].position, Quaternion.identity, id.muzzles[i].transform);
                    if (i == 0)
                    {
                        flash.GetComponent<AudioSource>().clip = weapon.sounds.shotSFX;
                        flash.GetComponent<AudioSource>().Play();
                    }
                }


                shots++;

                Crosshair.Instance.Resize(weapon.crosshairShootingSpread);

                VisualRecoil();

                yield return new WaitForSeconds(weapon.timeBetweenBullets);
            }
            yield break;
        }

        private void VisualRecoil()
        {
            weaponHolder.localPosition += weaponHolder.right * -weapon.visualRecoil;
        }
        #endregion

        #region reloading
        private void DefaultReload()
        {
            if (weapon != null && !reloading &&
                (InputManager.playerInputs.Reload && id.currentBullets < weapon.magazineSize
                || id.currentBullets <= 0 && weapon.autoReload && weapon.reloadingMethod != Weapon_SO.ReloadingMethod.Overheat) && id.totalBullets > 0) StartCoroutine(HandleReload());

        }
        private IEnumerator HandleReload()
        {
            reloading = true;

            events.onReload?.Invoke();

            SoundManager.Instance.PlaySound(weapon.sounds.reloadSFX, 1);

            if (id.GetComponentInChildren<Animator>()) id.GetComponentInChildren<Animator>().SetTrigger("Reload");

            yield return new WaitForSeconds(weapon.reloadTime);

            reloading = false;

            // Set the proper amount of bullets, depending on magazine type.
            if (!weapon.limitedMagazines) id.currentBullets = weapon.magazineSize;
            else
            {
                if (interactManager.inventoryMethod == InteractionManager.InventoryMethod.HotbarOnly)
                {
                    if (id.totalBullets > weapon.magazineSize) // You can still reload a full magazine
                    {
                        id.totalBullets = id.totalBullets - (weapon.magazineSize - id.currentBullets);
                        id.currentBullets = weapon.magazineSize;
                    }
                    else if (id.totalBullets == weapon.magazineSize) // You can only reload a single full magazine more
                    {
                        id.totalBullets = id.totalBullets - (weapon.magazineSize - id.currentBullets);
                        id.currentBullets = weapon.magazineSize;
                    }
                    else if (id.totalBullets < weapon.magazineSize) // You cant reload a whole magazine
                    {
                        int bulletsLeft = id.currentBullets;
                        if (id.currentBullets + id.totalBullets <= weapon.magazineSize)
                        {
                            id.currentBullets = id.currentBullets + id.totalBullets;
                            if (id.totalBullets - (weapon.magazineSize - bulletsLeft) >= 0) id.totalBullets = id.totalBullets - (weapon.magazineSize - bulletsLeft);
                            else id.totalBullets = 0;
                        }
                        else
                        {
                            int ToAdd = weapon.magazineSize - id.currentBullets;
                            id.currentBullets = id.currentBullets + ToAdd;
                            if (id.totalBullets - ToAdd >= 0) id.totalBullets = id.totalBullets - ToAdd;
                            else id.totalBullets = 0;
                        }
                    }
                }
                else
                {
                    CheckForBullets();
                    if (id.totalBullets >= weapon.magazineSize) // You can still reload a full magazine
                    {
                        UIController.instance.ReduceInventoryAmount(weapon.ammoType, weapon.magazineSize - id.currentBullets);
                        id.currentBullets = weapon.magazineSize;
                    }
                    else
                    {
                        int bulletsLeft = id.currentBullets;
                        if (id.currentBullets + id.totalBullets <= weapon.magazineSize)
                        {
                            id.currentBullets = id.currentBullets + id.totalBullets;
                            if (id.totalBullets - (weapon.magazineSize - bulletsLeft) >= 0) UIController.instance.ReduceInventoryAmount(weapon.ammoType, weapon.magazineSize - bulletsLeft);
                            else UIController.instance.ReduceInventoryAmount(weapon.ammoType, id.totalBullets);
                        }
                        else
                        {
                            int ToAdd = weapon.magazineSize - id.currentBullets;
                            id.currentBullets = id.currentBullets + ToAdd;
                            if (id.totalBullets - ToAdd >= 0) UIController.instance.ReduceInventoryAmount(weapon.ammoType, ToAdd);
                            else UIController.instance.ReduceInventoryAmount(weapon.ammoType, id.totalBullets);
                        }
                    }
                    CheckForBullets();
                }
            }

            UIController.updateWeaponInfo?.Invoke();
            yield break;
        }

        private void OverheatReload()
        {
            heatAmount = weapon.magazineSize - id.currentBullets;

            if (heatAmount >= weapon.magazineSize) reloading = true;

            if (reloading && heatAmount <= weapon.magazineSize * (weapon.allowShootingAfterCoolingPercentage / 100)) reloading = false;

            Invoke(nameof(RegenBulletUnit), 1 / weapon.coolSpeed);
            id.currentBullets = Mathf.Clamp(id.currentBullets, 0, weapon.magazineSize);

            UIController.updateWeaponInfo?.Invoke();
        }

        private void RegenBulletUnit()
        {
            CancelInvoke(nameof(RegenBulletUnit));
            id.currentBullets++;
        }


        public void CheckForBullets()
        {
            if (weapon == null) return;
            id.totalBullets = UIController.instance.CheckForBullets(weapon.ammoType);
            UIController.updateWeaponInfo?.Invoke();
        }
        #endregion

        #region aiming
        private void FreeAim()
        {
            if (DeviceDetection.Instance.mode == DeviceDetection.InputMode.Controller)
            {
                Vector2 rightStickInput = Gamepad.current.rightStick.ReadValue();

                // Calculate the aiming direction based on the right stick input.
                Vector2 _dir = rightStickInput.normalized;

                weaponHolder.right = _dir;
                aimingOrientation = weaponHolder.right;

                Vector2 _scale = weaponHolder.localScale;
                if (_dir.x < 0) _scale.y = -1;
                else _scale.y = 1;

                weaponHolder.localScale = _scale;
            }
            else
            {
                Vector2 mousePosition = playerCamera.ScreenToWorldPoint(InputManager.playerInputs.MousePos);
                Vector2 dir = (mousePosition - (Vector2)transform.position).normalized;
                weaponHolder.right = dir;
                aimingOrientation = weaponHolder.right;

                Vector2 scale = weaponHolder.localScale;
                if (dir.x < 0) scale.y = -1;
                else scale.y = 1;

                weaponHolder.localScale = scale;
            }
        }

        private void OrientationBasedAim()
        {
            Vector2 aimDir = player.Graphics.localScale.x < 0 ? new Vector2(1, 0) : new Vector2(-1, 0);
            weaponHolder.right = aimDir;

        }

        private void HorizontalAim()
        {
            if (DeviceDetection.Instance.mode == DeviceDetection.InputMode.Controller)
            {
                Vector2 rightStickInput = Gamepad.current.rightStick.ReadValue();

                // Calculate the aiming direction based on the right stick input.
                Vector2 _aimDir = new Vector2(rightStickInput.x, 0).normalized;

                if (_aimDir != Vector2.zero)
                {
                    weaponHolder.right = _aimDir;

                    aimingOrientation = weaponHolder.right;

                    Vector2 _scale = weaponHolder.localScale;

                    weaponHolder.localScale = _scale;
                }
            }
            else
            {
                Vector2 mousePosition = playerCamera.ScreenToWorldPoint(InputManager.playerInputs.MousePos);
                Vector2 dir = (mousePosition - (Vector2)transform.position).normalized;

                Vector2 aimDir = dir.x < 0 ? new Vector2(-1, 0) : new Vector2(1, 0);
                weaponHolder.right = aimDir;

                aimingOrientation = weaponHolder.right;
                Vector2 scale = weaponHolder.localScale;
                if (dir.x < 0) scale.y = -1;
                else scale.y = 1;
            }
        }

        private void BothAxisAim()
        {
            if (DeviceDetection.Instance.mode == DeviceDetection.InputMode.Controller)
            {
                Vector2 rightStickInput = Gamepad.current.rightStick.ReadValue();

                // Calculate the aiming direction based on the right stick input.
                Vector2 _aimDir = Vector2.zero;
                Vector2 _scale = weaponHolder.localScale;

                // Determine the aim direction based on the right stick input.
                if (Mathf.Abs(rightStickInput.x) > Mathf.Abs(rightStickInput.y))
                {
                    // Horizontal snap
                    _aimDir = rightStickInput.x < 0 ? new Vector2(-1, 0) : new Vector2(1, 0);
                    _scale.y = 1;
                }
                else
                {
                    // Vertical snap
                    _aimDir = rightStickInput.y < 0 ? new Vector2(0, -1) : new Vector2(0, 1);
                    _scale.y = rightStickInput.y < 0 ? -1 : 1;
                }

                if (_aimDir != Vector2.zero)
                {
                    // Apply the aim direction and scale transformations
                    weaponHolder.right = _aimDir;
                    aimingOrientation = weaponHolder.right;
                    weaponHolder.localScale = _scale;
                }
            }
            else
            {

                Vector2 mousePosition = playerCamera.ScreenToWorldPoint(InputManager.playerInputs.MousePos);
                Vector2 dir = (mousePosition - (Vector2)transform.position).normalized;

                Vector2 aimDir = Vector2.zero;
                Vector2 scale = weaponHolder.localScale;

                // Determine the aim direction based on the relative positions of the mouse and the player
                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                {
                    // Horizontal snap
                    aimDir = dir.x < 0 ? new Vector2(-1, 0) : new Vector2(1, 0);
                    scale.y = 1; ;
                }
                else
                {
                    // Vertical snap
                    aimDir = dir.y < 0 ? new Vector2(0, -1) : new Vector2(0, 1);
                    scale.y = dir.y < 0 ? -1 : 1;
                }

                if (aimDir != Vector2.zero)
                {
                    // Apply the aim direction and scale transformations
                    weaponHolder.right = aimDir;
                    aimingOrientation = weaponHolder.right;
                    weaponHolder.localScale = scale;
                }
            }
        }
        #endregion

        #region others

        public void UnholsterWeapon()
        {
            events.onUnholster?.Invoke();

            if (inventory[currentWeapon] != null)
                weapon = inventory[currentWeapon].weapon;
            else
            {
                foreach (var weaponInInventory in inventory)
                {
                    if (weaponInInventory != null) weaponInInventory.gameObject.SetActive(false);
                }
                weapon = null;
                id = null;
                UIController.updateWeaponInfo?.Invoke();
                return;
            }

            // Select what is the current shooting style
            switch (weapon.shootingStyle)
            {
                case Weapon_SO.ShootingStyle.Raycast:
                    shoot = RaycastShot;
                    ClearCustomMethods();
                    break;
                case Weapon_SO.ShootingStyle.Projectile:
                    shoot = ProjectileShot;
                    ClearCustomMethods();
                    break;
                case Weapon_SO.ShootingStyle.Melee:
                    shoot = MeleeShot;
                    ClearCustomMethods();
                    break;
                case Weapon_SO.ShootingStyle.Custom:

                    shoot = CustomShot;

                    for (int i = 0; i < customShot.Length; i++)
                    {
                        // Assign the on shoot event to the unity event to call it each time we fire
                        if (customShot[i].weapon == weapon)
                        {
                            onCustomShoot = customShot[i].OnShoot;
                            onCustomShooting = customShot[i].OnShooting;
                            onCustomNotShooting = customShot[i].OnNotShooting;
                        }
                    }
                    break;
            }

            if (!weapon.infiniteBullets && weapon.shootingStyle != Weapon_SO.ShootingStyle.Melee)
            {
                switch (weapon.reloadingMethod)
                {
                    case Weapon_SO.ReloadingMethod.Default:
                        reload = DefaultReload;
                        break;
                    case Weapon_SO.ReloadingMethod.Overheat:
                        reload = OverheatReload;
                        break;
                }
            }
            else reload = null;


            AssignAimingMethod();

            foreach (var weaponInInventory in inventory)
            {
                if (weaponInInventory != null) weaponInInventory.gameObject.SetActive(false);
            }

            if (inventory[currentWeapon] != null)
            {
                inventory[currentWeapon].gameObject.SetActive(true);
                id = inventory[currentWeapon];

                // Adapt the system to the current amount of bullets the picked up weapon has.
                id.currentBullets = inventory[currentWeapon].currentBullets;
                if (weapon.limitedMagazines)
                {
                    if (interactManager.inventoryMethod == InteractionManager.InventoryMethod.HotbarOnly)
                    {
                        id.totalBullets = inventory[currentWeapon].totalBullets;
                    }
                    else
                    {
                        CheckForBullets();
                    }

                }
                else id.totalBullets = id.currentBullets;
            }
            else
            {
                weapon = null;
                id = null;
            }

            SoundManager.Instance.PlaySound(weapon.sounds.unholsterSFX, 1);

            UIController.updateWeaponInfo?.Invoke();
        }



        private void ClearCustomMethods()
        {
            onCustomShoot = null;
            onCustomShooting = null;
            onCustomNotShooting = null;
        }

        private void AssignAimingMethod()
        {
            aim = null;

            switch (weapon.aimingMethod)
            {
                case Weapon_SO.AimingMethod.Horizontal:
                    aim = HorizontalAim;
                    break;
                case Weapon_SO.AimingMethod.BothAxis:
                    aim = BothAxisAim;
                    break;
                case Weapon_SO.AimingMethod.Free:
                    aim = FreeAim;
                    break;
                case Weapon_SO.AimingMethod.OrientationBased:
                    aim = OrientationBasedAim;
                    break;
            }
        }

        public void ReleaseCurrentWeapon()
        {
            Destroy(inventory[currentWeapon].gameObject);
            inventory[currentWeapon] = null;
            id = null;
        }

        public void HolsterCurrentWeapon()
        {
            inventory[currentWeapon].gameObject.SetActive(false);
            inventory[currentWeapon] = null;
            weapon = null;
            id = null;
        }

        private void WeaponInvReleaseCheck()
        {
            if (UIController.currentInventorySlot.isHotBarSlot && UIController.currentInventorySlot.slotData.inventoryItem == null && UIController.currentInventorySlot.id == currentWeapon)
            {
                HolsterCurrentWeapon();
            }

            if (UIController.highlightedInventorySlot.isHotBarSlot && UIController.highlightedInventorySlot.slotData.inventoryItem != null && UIController.highlightedInventorySlot.id == currentWeapon)
            {
                weapon = (Weapon_SO)UIController.highlightedInventorySlot.slotData.inventoryItem;
                inventory[UIController.highlightedInventorySlot.id] = UIController.highlightedInventorySlot.slotData.saveWeapon.GetComponent<WeaponIdentification>();
                UnholsterWeapon();
            }
        }

        public void HideWeapon()
        {
            if (inventory[currentWeapon] == null) return;
            inventory[currentWeapon].gameObject.SetActive(false);
            weaponHidden = true;
        }

        public void ShowWeapon()
        {
            if (inventory[currentWeapon] == null) return;
            inventory[currentWeapon].gameObject.SetActive(true);
            weaponHidden = false;
        }
        // Auxiliar variables

        private float amountCharged;

        private bool canReleaseAndShoot = false;

        private bool PlayerIsShooting()
        {
            if (weapon == null || !canShootWhileGliding && player.gliding || !canShootWhileLadder && player.ladder || !canShoot || reloading || id.currentBullets <= 0) return false;


            switch (weapon.shootingMethod)
            {
                case Weapon_SO.ShootingMethod.Press:
                    return InputManager.playerInputs.Shoot;

                case Weapon_SO.ShootingMethod.PressAndHold:
                    return InputManager.playerInputs.ShootHold;

                case Weapon_SO.ShootingMethod.ReleaseWhenReady:


                    if (InputManager.playerInputs.ShootHold) amountCharged += Time.deltaTime;
                    else amountCharged = 0;

                    if (amountCharged >= weapon.chargeRequiredToShoot) canReleaseAndShoot = true;

                    return !InputManager.playerInputs.ShootHold && canReleaseAndShoot;

                case Weapon_SO.ShootingMethod.ShootWhenReady:
                    if (InputManager.playerInputs.ShootHold) amountCharged += Time.deltaTime;
                    else amountCharged = 0;
                    return InputManager.playerInputs.ShootHold && amountCharged >= weapon.chargeRequiredToShoot;

                default:
                    return InputManager.playerInputs.Shoot;
            }
        }

        private void ResetShot()
        {
            canShoot = true;
            CancelInvoke(nameof(ResetShot));
        }

        private void Hit(Collider2D target, Vector2 location, Vector2 hitOrientation)
        {
            if (target.GetComponent<IDamageable>() != null) target.GetComponent<IDamageable>().Damage(weapon.damage * PlayerMultipliers.Instance.damageModifier);

            //int sortedPosition = sortedHitLayer[target.gameObject.layer];

            var hitEffect = Instantiate(weapon.hitEffects[0].hitVFX, location, Quaternion.identity);
            // Calculate the rotation to match the hit surface
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hitOrientation);
            // Apply the rotation to the hit effect
            hitEffect.transform.rotation = rotation;
            hitEffect.transform.parent = target.transform;
            Destroy(hitEffect, 2f);
        }

        private void SortHitLayer()
        {
            int sortedIndex = 0;
            for (int i = 0; i < 32; i++)
            {
                if (hitLayer == (hitLayer | (1 << i)))
                {
                    sortedHitLayer[i] = sortedIndex;
                    sortedIndex++;
                }
            }
        }


        private void GetInitialWeapons()
        {
            if (initialWeapons.Length == 0) return;
            int i = 0;
            while (i < initialWeapons.Length)
            {
                WeaponIdentification wp = Instantiate(initialWeapons[i].weaponObject, weaponHolder.position, Quaternion.identity, weaponHolder);
                wp.currentBullets = initialWeapons[i].magazineSize;
                id = wp;
                if (initialWeapons[i].limitedMagazines)
                {
                    if (interactManager.inventoryMethod == InteractionManager.InventoryMethod.HotbarOnly)
                    {
                        wp.totalBullets = initialWeapons[i].magazineSize * initialWeapons[i].amountOfMagazines;
                    }
                    else
                    {
                        CheckForBullets();
                    }

                }
                else id.totalBullets = initialWeapons[i].magazineSize;
                inventory[i] = wp;
                UIController.instance.hotbarList[i].GetComponent<InventorySlot>().slotData = new InventorySlot.SlotData
                {
                    inventoryItem = initialWeapons[i],
                    amount = 1,
                    saveWeapon = wp.gameObject
                };
                if (i == 0)
                {
                    weapon = initialWeapons[0];
                    UnholsterWeapon();
                }
                else wp.gameObject.SetActive(false);
                i++;
            }
        }


        public void SetCurrentWeapon(int newCurrentWeapon)
        {
            currentWeapon = newCurrentWeapon;
        }
        public void SetWeapon(Weapon_SO newWeapon)
        {
            weapon = newWeapon;
        }
        #endregion
    }

}