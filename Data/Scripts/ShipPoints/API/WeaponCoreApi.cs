using System;
using System.Collections.Generic;
using ProtoBuf;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.ModAPI;
using VRageMath;
using System.Collections.Immutable;
using Sandbox.Game.Entities;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Collections;
//using static CoreSystems.Api.WcApi.DamageHandlerHelper;

namespace WeaponCore.Api
{
    /// <summary>
    /// https://github.com/sstixrud/WeaponCore/blob/master/Data/Scripts/WeaponCore/Api/WeaponCoreApi.cs
    /// </summary>
    public partial class WcApi
    {
        private bool _apiInit;

        private Action<IList<byte[]>> _getAllWeaponDefinitions;
        private Action<ICollection<MyDefinitionId>> _getCoreWeapons;
        private Action<ICollection<MyDefinitionId>> _getNpcSafeWeapons;
        private Action<IDictionary<MyDefinitionId, List<MyTuple<int, MyTuple<MyDefinitionId, string, string, bool>>>>> _getAllWeaponMagazines;
        private Action<IDictionary<MyDefinitionId, List<MyTuple<int, MyTuple<MyDefinitionId, string, string, bool>>>>> _getAllNpcSafeWeaponMagazines;

        private Action<ICollection<MyDefinitionId>> _getCoreStaticLaunchers;
        private Action<ICollection<MyDefinitionId>> _getCoreTurrets;
        private Action<ICollection<MyDefinitionId>> _getCorePhantoms;
        private Action<ICollection<MyDefinitionId>> _getCoreRifles;
        private Action<IList<byte[]>> _getCoreArmors;

        private Action<long, int, Action<ListReader<MyTuple<ulong, long, int, MyEntity, MyEntity, ListReader<MyTuple<Vector3D, object, float>>>>>> _registerDamageEvent;
        private Func<long, bool, Func<MyEntity, IMyCharacter, long, int, bool>, bool> _targetFocusHandler;
        private Func<long, bool, Func<IMyCharacter, long, int, bool>, bool> _hudHandler;
        private Func<long, bool, Func<Vector3D, Vector3D, int, bool, object, int, int, int, bool>, bool> _shootHandler;
        private Action<MyEntity, int, Action<int, bool>> _monitorEvents;
        private Action<MyEntity, int, Action<int, bool>> _unmonitorEvents;
        private Action<MyEntity, int, Action<long, int, ulong, long, Vector3D, bool>> _addProjectileMonitor;
        private Action<MyEntity, int, Action<long, int, ulong, long, Vector3D, bool>> _removeProjectileMonitor;

        private Func<MyEntity, object, int, double, bool> _shootRequest;

        private Func<ulong, MyTuple<Vector3D, Vector3D, float, float, long, string>> _getProjectileState;
        private Action<ulong, MyTuple<bool, Vector3D, Vector3D, float>> _setProjectileState;

        private Action<MyEntity, ICollection<MyTuple<MyEntity, float>>> _getSortedThreats;
        private Action<MyEntity, ICollection<MyEntity>> _getObstructions;
        private Func<MyEntity, int, MyEntity> _getAiFocus;
        private Func<MyEntity, MyEntity, int, bool> _setAiFocus;
        private Func<MyEntity, long, bool> _releaseAiFocus;
        private Func<MyEntity, bool> _hasAi;
        private Func<MyEntity, bool> _hasCoreWeapon;

        private Func<MyEntity, bool> _toggoleInfiniteResources;
        private Action<MyEntity> _disableRequiredPower;

        private Func<MyEntity, long> _getPlayerController;
        private Func<MyEntity, MyTuple<bool, int, int>> _getProjectilesLockedOn;
        private Func<MyDefinitionId, float> _getMaxPower;

        private Func<MyEntity, float> _getOptimalDps;
        private Func<MyEntity, float> _getConstructEffectiveDps;
        private Func<MyEntity, MyTuple<bool, bool>> _isInRange;

        private Func<MyEntity, int, MyTuple<bool, bool, bool, MyEntity>> _getWeaponTarget;
        private Action<MyEntity, MyEntity, int> _setWeaponTarget;
        private Action<MyEntity, bool, int> _fireWeaponOnce;
        private Action<MyEntity, bool, bool, int> _toggleWeaponFire;
        private Func<MyEntity, int, bool, bool, bool> _isWeaponReadyToFire;
        private Func<MyEntity, int, float> _getMaxWeaponRange;
        private Func<MyEntity, ICollection<string>, int, bool> _getTurretTargetTypes;
        private Action<MyEntity, ICollection<string>, int> _setTurretTargetTypes;
        private Action<MyEntity, float> _setBlockTrackingRange;
        private Func<MyEntity, MyEntity, int, bool> _isTargetAligned;
        private Func<MyEntity, MyEntity, int, MyTuple<bool, Vector3D?>> _isTargetAlignedExtended;
        private Func<MyEntity, MyEntity, int, bool> _canShootTarget;
        private Func<MyEntity, MyEntity, int, Vector3D?> _getPredictedTargetPos;
        private Func<MyEntity, float> _getHeatLevel;
        private Func<MyEntity, float> _currentPowerConsumption;
        private Func<MyEntity, int, string> _getActiveAmmo;
        private Action<MyEntity, int, string> _setActiveAmmo;

        private Func<MyEntity, int, Matrix> _getWeaponAzimuthMatrix;
        private Func<MyEntity, int, Matrix> _getWeaponElevationMatrix;
        private Func<MyEntity, MyEntity, bool, bool, bool> _isTargetValid;
        private Func<MyEntity, int, bool> _isWeaponShooting;
        private Func<MyEntity, int, int> _getShotsFired;
        private Action<MyEntity, int, List<MyTuple<Vector3D, Vector3D, Vector3D, Vector3D, MatrixD, MatrixD>>> _getMuzzleInfo;
        private Func<MyEntity, int, MyTuple<Vector3D, Vector3D>> _getWeaponScope;
        private Func<MyEntity, int, MyTuple<MyDefinitionId, string, string, bool>> _getMagazineMap;
        private Func<MyEntity, int, MyDefinitionId, bool, bool> _setMagazine;
        private Func<MyEntity, int, bool> _forceReload;

        public void SetWeaponTarget(MyEntity weapon, MyEntity target, int weaponId = 0) =>
            _setWeaponTarget?.Invoke(weapon, target, weaponId);

        public void FireWeaponOnce(MyEntity weapon, bool allWeapons = true, int weaponId = 0) =>
            _fireWeaponOnce?.Invoke(weapon, allWeapons, weaponId);

        public void ToggleWeaponFire(MyEntity weapon, bool on, bool allWeapons, int weaponId = 0) =>
            _toggleWeaponFire?.Invoke(weapon, on, allWeapons, weaponId);

        public bool IsWeaponReadyToFire(MyEntity weapon, int weaponId = 0, bool anyWeaponReady = true,
            bool shootReady = false) =>
            _isWeaponReadyToFire?.Invoke(weapon, weaponId, anyWeaponReady, shootReady) ?? false;

        public float GetMaxWeaponRange(MyEntity weapon, int weaponId) =>
            _getMaxWeaponRange?.Invoke(weapon, weaponId) ?? 0f;

        public bool GetTurretTargetTypes(MyEntity weapon, IList<string> collection, int weaponId = 0) =>
            _getTurretTargetTypes?.Invoke(weapon, collection, weaponId) ?? false;

        public void SetTurretTargetTypes(MyEntity weapon, IList<string> collection, int weaponId = 0) =>
            _setTurretTargetTypes?.Invoke(weapon, collection, weaponId);

        public void SetBlockTrackingRange(MyEntity weapon, float range) =>
            _setBlockTrackingRange?.Invoke(weapon, range);

        public bool IsTargetAligned(MyEntity weapon, MyEntity targetEnt, int weaponId) =>
            _isTargetAligned?.Invoke(weapon, targetEnt, weaponId) ?? false;

        public MyTuple<bool, Vector3D?> IsTargetAlignedExtended(MyEntity weapon, MyEntity targetEnt, int weaponId) =>
            _isTargetAlignedExtended?.Invoke(weapon, targetEnt, weaponId) ?? new MyTuple<bool, Vector3D?>();

        public bool CanShootTarget(MyEntity weapon, MyEntity targetEnt, int weaponId) =>
            _canShootTarget?.Invoke(weapon, targetEnt, weaponId) ?? false;

        public Vector3D? GetPredictedTargetPosition(MyEntity weapon, MyEntity targetEnt, int weaponId) =>
            _getPredictedTargetPos?.Invoke(weapon, targetEnt, weaponId) ?? null;

        public float GetHeatLevel(MyEntity weapon) => _getHeatLevel?.Invoke(weapon) ?? 0f;
        public float GetCurrentPower(MyEntity weapon) => _currentPowerConsumption?.Invoke(weapon) ?? 0f;
        public void DisableRequiredPower(MyEntity weapon) => _disableRequiredPower?.Invoke(weapon);
        public bool HasCoreWeapon(MyEntity weapon) => _hasCoreWeapon?.Invoke(weapon) ?? false;

        public string GetActiveAmmo(MyEntity weapon, int weaponId) =>
            _getActiveAmmo?.Invoke(weapon, weaponId) ?? null;

        public void SetActiveAmmo(MyEntity weapon, int weaponId, string ammoType) =>
            _setActiveAmmo?.Invoke(weapon, weaponId, ammoType);

        public long GetPlayerController(MyEntity weapon) => _getPlayerController?.Invoke(weapon) ?? -1;

        public Matrix GetWeaponAzimuthMatrix(MyEntity weapon, int weaponId) =>
            _getWeaponAzimuthMatrix?.Invoke(weapon, weaponId) ?? Matrix.Zero;

        public Matrix GetWeaponElevationMatrix(MyEntity weapon, int weaponId) =>
            _getWeaponElevationMatrix?.Invoke(weapon, weaponId) ?? Matrix.Zero;

        public bool IsTargetValid(MyEntity weapon, MyEntity target, bool onlyThreats, bool checkRelations) =>
            _isTargetValid?.Invoke(weapon, target, onlyThreats, checkRelations) ?? false;

        public void GetAllWeaponDefinitions(IList<byte[]> collection) => _getAllWeaponDefinitions?.Invoke(collection);
        public void GetAllCoreWeapons(ICollection<MyDefinitionId> collection) => _getCoreWeapons?.Invoke(collection);
        public void GetNpcSafeWeapons(ICollection<MyDefinitionId> collection) => _getNpcSafeWeapons?.Invoke(collection);

        public void GetAllCoreStaticLaunchers(ICollection<MyDefinitionId> collection) => _getCoreStaticLaunchers?.Invoke(collection);
        public void GetAllWeaponMagazines(IDictionary<MyDefinitionId, List<MyTuple<int, MyTuple<MyDefinitionId, string, string, bool>>>> collection) => _getAllWeaponMagazines?.Invoke(collection);
        public void GetAllNpcSafeWeaponMagazines(IDictionary<MyDefinitionId, List<MyTuple<int, MyTuple<MyDefinitionId, string, string, bool>>>> collection) => _getAllNpcSafeWeaponMagazines?.Invoke(collection);

        public void GetAllCoreTurrets(ICollection<MyDefinitionId> collection) => _getCoreTurrets?.Invoke(collection);
        public void GetAllCorePhantoms(ICollection<MyDefinitionId> collection) => _getCorePhantoms?.Invoke(collection);
        public void GetAllCoreRifles(ICollection<MyDefinitionId> collection) => _getCoreRifles?.Invoke(collection);
        public void GetAllCoreArmors(IList<byte[]> collection) => _getCoreArmors?.Invoke(collection);

        public MyTuple<bool, int, int> GetProjectilesLockedOn(MyEntity victim) =>
            _getProjectilesLockedOn?.Invoke(victim) ?? new MyTuple<bool, int, int>();
        public void GetSortedThreats(MyEntity shooter, ICollection<MyTuple<MyEntity, float>> collection) =>
            _getSortedThreats?.Invoke(shooter, collection);
        public void GetObstructions(MyEntity shooter, ICollection<MyEntity> collection) =>
            _getObstructions?.Invoke(shooter, collection);
        public MyEntity GetAiFocus(MyEntity shooter, int priority = 0) => _getAiFocus?.Invoke(shooter, priority);
        public bool SetAiFocus(MyEntity shooter, MyEntity target, int priority = 0) =>
            _setAiFocus?.Invoke(shooter, target, priority) ?? false;
        public bool ReleaseAiFocus(MyEntity shooter, long playerId) =>
            _releaseAiFocus?.Invoke(shooter, playerId) ?? false;
        public MyTuple<bool, bool, bool, MyEntity> GetWeaponTarget(MyEntity weapon, int weaponId = 0) =>
            _getWeaponTarget?.Invoke(weapon, weaponId) ?? new MyTuple<bool, bool, bool, MyEntity>();
        public float GetMaxPower(MyDefinitionId weaponDef) => _getMaxPower?.Invoke(weaponDef) ?? 0f;
        public bool HasAi(MyEntity entity) => _hasAi?.Invoke(entity) ?? false;
        public float GetOptimalDps(MyEntity entity) => _getOptimalDps?.Invoke(entity) ?? 0f;
        public MyTuple<Vector3D, Vector3D, float, float, long, string> GetProjectileState(ulong projectileId) =>
            _getProjectileState?.Invoke(projectileId) ?? new MyTuple<Vector3D, Vector3D, float, float, long, string>();

        public float GetConstructEffectiveDps(MyEntity entity) => _getConstructEffectiveDps?.Invoke(entity) ?? 0f;
        public MyTuple<Vector3D, Vector3D> GetWeaponScope(MyEntity weapon, int weaponId) =>
            _getWeaponScope?.Invoke(weapon, weaponId) ?? new MyTuple<Vector3D, Vector3D>();

        public void AddProjectileCallback(MyEntity entity, int weaponId, Action<long, int, ulong, long, Vector3D, bool> action) =>
            _addProjectileMonitor?.Invoke(entity, weaponId, action);

        public void RemoveProjectileCallback(MyEntity entity, int weaponId, Action<long, int, ulong, long, Vector3D, bool> action) =>
            _removeProjectileMonitor?.Invoke(entity, weaponId, action);


        // block/grid/player, Threat, Other 
        public MyTuple<bool, bool> IsInRange(MyEntity entity) =>
            _isInRange?.Invoke(entity) ?? new MyTuple<bool, bool>();

        /// <summary>
        /// Set projectile values *Warning* be sure to pass in Vector3D.MinValue or float.MinValue to NOT set that value.
        /// bool = EndNow
        /// Vector3D Position
        /// Vector3D Additive velocity
        /// float BaseDamagePool
        /// </summary>
        /// <param name="projectileId"></param>
        /// <param name="values"></param>
        public void SetProjectileState(ulong projectileId, MyTuple<bool, Vector3D, Vector3D, float> values) =>
            _setProjectileState?.Invoke(projectileId, values);

        /// <summary>
        /// Gets whether the weapon is shooting, used by Hakerman's Beam Logic
        /// Unexpected behavior may occur when using this method
        /// </summary>
        /// <param name="weaponBlock"></param>
        /// <param name="weaponId"></param>
        /// <returns></returns>
        internal bool IsWeaponShooting(MyEntity weaponBlock, int weaponId) => _isWeaponShooting?.Invoke(weaponBlock, weaponId) ?? false;

        /// <summary>
        /// Gets how many shots the weapon fired, used by Hakerman's Beam Logic
        /// Unexpected behavior may occur when using this method
        /// </summary>
        /// <param name="weaponBlock"></param>
        /// <param name="weaponId"></param>
        /// <returns></returns>
        internal int GetShotsFired(MyEntity weaponBlock, int weaponId) => _getShotsFired?.Invoke(weaponBlock, weaponId) ?? -1;

        /// <summary>
        /// Gets the info of the weapon's all muzzles, used by Hakerman's Beam Logic
        /// returns: A list that contains every muzzle's Position, LocalPosition, Direction, UpDirection, ParentMatrix, DummyMatrix
        /// Unexpected behavior may occur when using this method
        /// </summary>
        /// <param name="weaponBlock"></param>
        /// <param name="weaponId"></param>
        /// <returns></returns>
        internal void GetMuzzleInfo(MyEntity weaponBlock, int weaponId, List<MyTuple<Vector3D, Vector3D, Vector3D, Vector3D, MatrixD, MatrixD>> output) =>
            _getMuzzleInfo?.Invoke(weaponBlock, weaponId, output);

        /// <summary>
        /// Entity can be a weapon or a grid/player (enables on all subgrids as well)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool ToggleInfiniteResources(MyEntity entity) =>
            _toggoleInfiniteResources?.Invoke(entity) ?? false;

        /// <summary>
        /// Monitor various kind of events, see WcApiDef.WeaponDefinition.AnimationDef.PartAnimationSetDef.EventTriggers for int mapping, bool is for active/inactive
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="partId"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public void MonitorEvents(MyEntity entity, int partId, Action<int, bool> action) =>
            _monitorEvents?.Invoke(entity, partId, action);

        /// <summary>
        /// Monitor various kind of events, see WcApiDef.WeaponDefinition.AnimationDef.PartAnimationSetDef.EventTriggers for int mapping, bool is for active/inactive
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="partId"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public void UnMonitorEvents(MyEntity entity, int partId, Action<int, bool> action) =>
            _unmonitorEvents?.Invoke(entity, partId, action);

        /// <summary>
        /// Monitor all weaponcore damage
        /// </summary>
        /// <param name="modId"></param>
        /// <param name="type"></param>  0 unregister, 1 register
        /// <param name="callback"></param>  object casts (ulong = projectileId, IMySlimBlock, MyFloatingObject, IMyCharacter, MyVoxelBase, MyPlanet, MyEntity Shield see next line)
        ///                                  You can detect the shield entity in a performant way by creating a hash check ShieldHash = MyStringHash.GetOrCompute("DefenseShield");
        ///                                  then use it by Session.ShieldApiLoaded && Session.ShieldHash == ent.DefinitionId?.SubtypeId && ent.Render.Visible;  Visible means shield online
        public void RegisterDamageEvent(long modId, int type, Action<ListReader<MyTuple<ulong, long, int, MyEntity, MyEntity, ListReader<MyTuple<Vector3D, object, float>>>>> callback)
        {
            _registerDamageEvent?.Invoke(modId, type, callback);
        }

        /// <summary>
        /// This allows you to determine when and if a player can modify the current target focus on a player/grid/phantonm. Use only on server
        /// </summary>
        /// <param name="handledEntityId"> is the player/grid/phantom you want to control the target focus for, applies to subgrids as well</param>
        /// <param name="unregister"> be sure to unregister when you no longer want to receive callbacks</param>
        public void TargetFocushandler(long handledEntityId, bool unregister)
        {
            _targetFocusHandler(handledEntityId, unregister, TargetFocusCallback);
        }

        /// <summary>
        /// This callback fires whenever a player attempts to modify the target focus
        /// </summary>
        /// <param name="target"></param>
        /// <param name="requestingCharacter"></param>
        /// <param name="handledEntityId"></param>
        /// <param name="modeCode"></param>
        /// <returns></returns>
        private bool TargetFocusCallback(MyEntity target, IMyCharacter requestingCharacter, long handledEntityId, int modeCode)
        {
            var mode = (ChangeMode)modeCode;

            return true;
        }

        public enum ChangeMode
        {
            Add,
            Release,
            Lock,
        }
        /// <summary>
        ///  Enables you to allow/deny hud draw requests.  Do not use this on dedicated server.
        /// </summary>
        /// <param name="handledEntityId"></param>
        /// <param name="unregister"></param>
        public void Hudhandler(long handledEntityId, bool unregister)
        {
            _hudHandler?.Invoke(handledEntityId, unregister, HudCallback);
        }

        /// <summary>
        /// This callback fires whenever the hud tries to update
        /// </summary>
        /// <param name="requestingCharacter"></param>
        /// <param name="handledEntityId"></param>
        /// <param name="modeCode"></param>
        /// <returns></returns>
        private bool HudCallback(IMyCharacter requestingCharacter, long handledEntityId, int modeCode)
        {
            var mode = (HudMode)modeCode;

            return true;
        }

        internal enum HudMode
        {
            Selector,
            Reload,
            TargetInfo,
            Lead,
            Drone,
            PainterMarks,
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weaponEntity"></param>
        /// <param name="target">MyEntity, ulong (projectile) or Vector3D</param>
        /// <param name="weaponId">Most weapons have only id 0, but some are multi-weapon entities</param>
        /// <param name="additionalDeviateShotAngle"></param>
        /// <returns></returns>
        public bool ShootRequest(MyEntity weaponEntity, object target, int weaponId = 0, double additionalDeviateShotAngle = 0) => _shootRequest?.Invoke(weaponEntity, target, weaponId, additionalDeviateShotAngle) ?? false;

        /// <summary>
        /// Enables you to monitor and approve shoot requests for this weapon/construct/player/grid network
        /// </summary>
        /// <param name="handledEntityId"></param>
        /// <param name="unregister"></param>
        /// <param name="callback"></param>
        public void ShootRequestHandler(long handledEntityId, bool unregister, Func<Vector3D, Vector3D, int, bool, object, int, int, int, bool> callback)
        {
            _shootHandler?.Invoke(handledEntityId, unregister, callback); // see example callback below
        }

        /// <summary>
        /// This callback fires whenever a shoot request is being evaluated for against a success criteria or is pending some action 
        /// </summary>
        /// <param name="scopePos"></param>
        /// <param name="scopeDirection"></param>
        /// <param name="requestState">This is the state of your request, state 0 means proceeding as requested</param>
        /// <param name="hasLos">This is false if wc thinks the target is occluded, you can choose to allow it to proceed anyway or not</param>
        /// <param name="target"> valid objects to cast too are MyEntity, ulong (projectile ids) and target Vector3Ds</param>
        /// <param name="currentAmmo"></param>
        /// <param name="remainingMags"></param>
        /// <param name="requestStage">The number of times this callback will fire will depend on the relevant firing stages for this weapon/ammo and how far it gets</param>
        /// <returns></returns>
        private bool ShootCallBack(Vector3D scopePos, Vector3D scopeDirection, int requestState, bool hasLos, object target, int currentAmmo, int remainingMags, int requestStage)
        {
            var stage = (EventTriggers)requestStage;
            var state = (ShootState)requestState;
            var targetAsEntity = target as MyEntity;
            var targetAsProjectileId = target as ulong? ?? 0;
            var targetAsPosition = target as Vector3D? ?? Vector3D.Zero;

            return true;
        }

        public enum ShootState
        {
            EventStart,
            EventEnd,
            Preceding,
            Canceled,
        }

        public enum EventTriggers
        {
            Reloading,
            Firing,
            Tracking,
            Overheated,
            TurnOn,
            TurnOff,
            BurstReload,
            NoMagsToLoad,
            PreFire,
            EmptyOnGameLoad,
            StopFiring,
            StopTracking,
            LockDelay,
            Init,
            Homing,
            TargetAligned,
            WhileOn,
            TargetRanged100,
            TargetRanged75,
            TargetRanged50,
            TargetRanged25,
        }

        /// <summary>
        /// Get active ammo Mag map from weapon 
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="weaponId"></param>
        /// <returns>Mag definitionId, mag name, ammoRound name, weapon must aim (not manual aim) true/false</returns>
        public MyTuple<MyDefinitionId, string, string, bool> GetMagazineMap(MyEntity weapon, int weaponId)
        {
            return _getMagazineMap?.Invoke(weapon, weaponId) ?? new MyTuple<MyDefinitionId, string, string, bool>();
        }

        /// <summary>
        ///  Set the active ammo type via passing Mag DefinitionId
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="weaponId"></param>
        /// <param name="id"></param>
        /// <param name="forceReload"></param>
        /// <returns></returns>
        public bool SetMagazine(MyEntity weapon, int weaponId, MyDefinitionId id, bool forceReload)
        {
            return _setMagazine?.Invoke(weapon, weaponId, id, forceReload) ?? false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="weaponId"></param>
        /// <returns></returns>
        public bool ForceReload(MyEntity weapon, int weaponId)
        {
            return _forceReload?.Invoke(weapon, weaponId) ?? false;

        }

        private const long Channel = 67549756549;
        private bool _getWeaponDefinitions;
        private bool _isRegistered;
        private Action _readyCallback;

        /// <summary>
        /// True if CoreSystems replied when <see cref="Load"/> got called.
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// Only filled if giving true to <see cref="Load"/>.
        /// </summary>
        public readonly List<WcApiDef.WeaponDefinition> WeaponDefinitions = new List<WcApiDef.WeaponDefinition>();

        /// <summary>
        /// Ask CoreSystems to send the API methods.
        /// <para>Throws an exception if it gets called more than once per session without <see cref="Unload"/>.</para>
        /// </summary>
        /// <param name="readyCallback">Method to be called when CoreSystems replies.</param>
        /// <param name="getWeaponDefinitions">Set to true to fill <see cref="WeaponDefinitions"/>.</param>
        public void Load(Action readyCallback = null, bool getWeaponDefinitions = false)
        {
            if (_isRegistered)
                throw new Exception($"{GetType().Name}.Load() should not be called multiple times!");

            _readyCallback = readyCallback;
            _getWeaponDefinitions = getWeaponDefinitions;
            _isRegistered = true;
            MyAPIGateway.Utilities.RegisterMessageHandler(Channel, HandleMessage);
            MyAPIGateway.Utilities.SendModMessage(Channel, "ApiEndpointRequest");
        }

        public void Unload()
        {
            MyAPIGateway.Utilities.UnregisterMessageHandler(Channel, HandleMessage);

            ApiAssign(null);

            _isRegistered = false;
            _apiInit = false;
            IsReady = false;
        }

        private void HandleMessage(object obj)
        {
            if (_apiInit || obj is string
            ) // the sent "ApiEndpointRequest" will also be received here, explicitly ignoring that
                return;

            var dict = obj as IReadOnlyDictionary<string, Delegate>;

            if (dict == null)
                return;

            ApiAssign(dict, _getWeaponDefinitions);

            IsReady = true;
            _readyCallback?.Invoke();
        }

        public void ApiAssign(IReadOnlyDictionary<string, Delegate> delegates, bool getWeaponDefinitions = false)
        {
            _apiInit = (delegates != null);
            /// base methods
            AssignMethod(delegates, "GetAllWeaponDefinitions", ref _getAllWeaponDefinitions);
            AssignMethod(delegates, "GetCoreWeapons", ref _getCoreWeapons);
            AssignMethod(delegates, "GetNpcSafeWeapons", ref _getNpcSafeWeapons);

            AssignMethod(delegates, "GetAllWeaponMagazines", ref _getAllWeaponMagazines);
            AssignMethod(delegates, "GetAllNpcSafeWeaponMagazines", ref _getAllNpcSafeWeaponMagazines);

            AssignMethod(delegates, "GetCoreStaticLaunchers", ref _getCoreStaticLaunchers);
            AssignMethod(delegates, "GetCoreTurrets", ref _getCoreTurrets);
            AssignMethod(delegates, "GetCorePhantoms", ref _getCorePhantoms);
            AssignMethod(delegates, "GetCoreRifles", ref _getCoreRifles);
            AssignMethod(delegates, "GetCoreArmors", ref _getCoreArmors);

            //AssignMethod(delegates, "GetBlockWeaponMap", ref _getBlockWeaponMap);
            AssignMethod(delegates, "GetSortedThreatsBase", ref _getSortedThreats);
            AssignMethod(delegates, "GetObstructionsBase", ref _getObstructions);
            AssignMethod(delegates, "GetMaxPower", ref _getMaxPower);
            AssignMethod(delegates, "GetProjectilesLockedOnBase", ref _getProjectilesLockedOn);
            AssignMethod(delegates, "GetAiFocusBase", ref _getAiFocus);
            AssignMethod(delegates, "SetAiFocusBase", ref _setAiFocus);
            AssignMethod(delegates, "ReleaseAiFocusBase", ref _releaseAiFocus);
            AssignMethod(delegates, "HasGridAiBase", ref _hasAi);
            AssignMethod(delegates, "GetOptimalDpsBase", ref _getOptimalDps);
            AssignMethod(delegates, "GetConstructEffectiveDpsBase", ref _getConstructEffectiveDps);
            AssignMethod(delegates, "IsInRangeBase", ref _isInRange);
            AssignMethod(delegates, "GetProjectileState", ref _getProjectileState);
            AssignMethod(delegates, "SetProjectileState", ref _setProjectileState);

            AssignMethod(delegates, "AddMonitorProjectile", ref _addProjectileMonitor);
            AssignMethod(delegates, "RemoveMonitorProjectile", ref _removeProjectileMonitor);

            AssignMethod(delegates, "TargetFocusHandler", ref _targetFocusHandler);
            AssignMethod(delegates, "HudHandler", ref _hudHandler);
            AssignMethod(delegates, "ShootHandler", ref _shootHandler);
            AssignMethod(delegates, "ShootRequest", ref _shootRequest);

            /// block methods
            AssignMethod(delegates, "GetWeaponTargetBase", ref _getWeaponTarget);
            AssignMethod(delegates, "SetWeaponTargetBase", ref _setWeaponTarget);
            AssignMethod(delegates, "FireWeaponOnceBase", ref _fireWeaponOnce);
            AssignMethod(delegates, "ToggleWeaponFireBase", ref _toggleWeaponFire);
            AssignMethod(delegates, "IsWeaponReadyToFireBase", ref _isWeaponReadyToFire);
            AssignMethod(delegates, "GetMaxWeaponRangeBase", ref _getMaxWeaponRange);
            AssignMethod(delegates, "GetTurretTargetTypesBase", ref _getTurretTargetTypes);
            AssignMethod(delegates, "SetTurretTargetTypesBase", ref _setTurretTargetTypes);
            AssignMethod(delegates, "SetBlockTrackingRangeBase", ref _setBlockTrackingRange);
            AssignMethod(delegates, "IsTargetAlignedBase", ref _isTargetAligned);
            AssignMethod(delegates, "IsTargetAlignedExtendedBase", ref _isTargetAlignedExtended);
            AssignMethod(delegates, "CanShootTargetBase", ref _canShootTarget);
            AssignMethod(delegates, "GetPredictedTargetPositionBase", ref _getPredictedTargetPos);
            AssignMethod(delegates, "GetHeatLevelBase", ref _getHeatLevel);
            AssignMethod(delegates, "GetCurrentPowerBase", ref _currentPowerConsumption);
            AssignMethod(delegates, "DisableRequiredPowerBase", ref _disableRequiredPower);
            AssignMethod(delegates, "HasCoreWeaponBase", ref _hasCoreWeapon);
            AssignMethod(delegates, "GetActiveAmmoBase", ref _getActiveAmmo);
            AssignMethod(delegates, "SetActiveAmmoBase", ref _setActiveAmmo);
            AssignMethod(delegates, "GetPlayerControllerBase", ref _getPlayerController);
            AssignMethod(delegates, "GetWeaponAzimuthMatrixBase", ref _getWeaponAzimuthMatrix);
            AssignMethod(delegates, "GetWeaponElevationMatrixBase", ref _getWeaponElevationMatrix);
            AssignMethod(delegates, "IsTargetValidBase", ref _isTargetValid);
            AssignMethod(delegates, "GetWeaponScopeBase", ref _getWeaponScope);

            //Hakerman's Beam Logic
            AssignMethod(delegates, "IsWeaponShootingBase", ref _isWeaponShooting);
            AssignMethod(delegates, "GetShotsFiredBase", ref _getShotsFired);
            AssignMethod(delegates, "GetMuzzleInfoBase", ref _getMuzzleInfo);
            AssignMethod(delegates, "ToggleInfiniteAmmoBase", ref _toggoleInfiniteResources);
            AssignMethod(delegates, "RegisterEventMonitor", ref _monitorEvents);
            AssignMethod(delegates, "UnRegisterEventMonitor", ref _unmonitorEvents);
            AssignMethod(delegates, "GetMagazineMap", ref _getMagazineMap);

            AssignMethod(delegates, "SetMagazine", ref _setMagazine);
            AssignMethod(delegates, "ForceReload", ref _forceReload);

            // Damage handler
            AssignMethod(delegates, "DamageHandler", ref _registerDamageEvent);

            if (getWeaponDefinitions)
            {
                var byteArrays = new List<byte[]>();
                GetAllWeaponDefinitions(byteArrays);
                foreach (var byteArray in byteArrays)
                    WeaponDefinitions.Add(MyAPIGateway.Utilities.SerializeFromBinary<WcApiDef.WeaponDefinition>(byteArray));
            }
        }

        private void AssignMethod<T>(IReadOnlyDictionary<string, Delegate> delegates, string name, ref T field)
            where T : class
        {
            if (delegates == null)
            {
                field = null;
                return;
            }

            Delegate del;
            if (!delegates.TryGetValue(name, out del))
                throw new Exception($"{GetType().Name} :: Couldn't find {name} delegate of type {typeof(T)}");

            field = del as T;

            if (field == null)
                throw new Exception(
                    $"{GetType().Name} :: Delegate {name} is not type {typeof(T)}, instead it's: {del.GetType()}");
        }

        public class DamageHandlerHelper
        {
            public void YourCallBackFunction(List<ProjectileDamageEvent> list)
            {
                // Your code goes here
                //
                // Once this function completes the data in the list will be deleted... if you need to use the data in this list
                // after this function completes make a copy of it.
                //
                // This is setup to be easy to use.  If you need more performance modify the Default Callback for your purposes and avoid
                // copying callbacks into new lists with ProjectileDamageEvent structs.  Note that the ListReader will remain usable for only 1 tick, then it will be cleared by wc.
                //
            }


            /// Don't touch anything below this line
            public void RegisterForDamage(long modId, EventType type)
            {
                _wcApi.RegisterDamageEvent(modId, (int) type, DefaultCallBack);
            }

            private void DefaultCallBack(ListReader<MyTuple<ulong, long, int, MyEntity, MyEntity, ListReader<MyTuple<Vector3D, object, float>>>> listReader)
            {
                YourCallBackFunction(ProcessEvents(listReader));
                CleanUpEvents();
            }

            private readonly List<ProjectileDamageEvent> _convertedObjects = new List<ProjectileDamageEvent>();
            private readonly Stack<List<ProjectileDamageEvent.ProHit>> _hitPool = new Stack<List<ProjectileDamageEvent.ProHit>>(256);

            private List<ProjectileDamageEvent> ProcessEvents(ListReader<MyTuple<ulong, long, int, MyEntity, MyEntity, ListReader<MyTuple<Vector3D, object, float>>>> projectiles)
            {
                foreach (var p in projectiles)
                {
                    var hits = _hitPool.Count > 0 ? _hitPool.Pop() : new List<ProjectileDamageEvent.ProHit>();

                    foreach (var hitObj in p.Item6)
                    {
                        hits.Add(new ProjectileDamageEvent.ProHit { HitPosition = hitObj.Item1, ObjectHit = hitObj.Item2, Damage = hitObj.Item3 });
                    }
                    _convertedObjects.Add(new ProjectileDamageEvent { ProId = p.Item1, PlayerId = p.Item2, WeaponId = p.Item3, WeaponEntity = p.Item4, WeaponParent = p.Item5, ObjectsHit = hits });
                }

                return _convertedObjects;
            }

            private void CleanUpEvents()
            {
                foreach (var p in _convertedObjects)
                {
                    p.ObjectsHit.Clear();
                    _hitPool.Push(p.ObjectsHit);
                }
                _convertedObjects.Clear();
            }

            public struct ProjectileDamageEvent
            {
                public ulong ProId;
                public long PlayerId;
                public int WeaponId;
                public MyEntity WeaponEntity;
                public MyEntity WeaponParent;
                public List<ProHit> ObjectsHit;

                public struct ProHit
                {
                    public Vector3D HitPosition; // To == first hit, From = projectile start position this frame
                    public object ObjectHit; // block, player, etc... 
                    public float Damage;
                }
            }


            private readonly WcApi _wcApi;
            public DamageHandlerHelper(WcApi wcApi)
            {
                _wcApi = wcApi;
            }

            public enum EventType
            {
                Unregister,
                SystemWideDamageEvents,
            }
        }

    }

    public static class WcApiDef
    {
        [ProtoContract]
        public class ContainerDefinition
        {
            [ProtoMember(1)] internal WeaponDefinition[] WeaponDefs;
            [ProtoMember(2)] internal ArmorDefinition[] ArmorDefs;
            [ProtoMember(3)] internal UpgradeDefinition[] UpgradeDefs;
            [ProtoMember(4)] internal SupportDefinition[] SupportDefs;
        }

        [ProtoContract]
        public class ConsumeableDef
        {
            [ProtoMember(1)] internal string ItemName;
            [ProtoMember(2)] internal string InventoryItem;
            [ProtoMember(3)] internal int ItemsNeeded;
            [ProtoMember(4)] internal bool Hybrid;
            [ProtoMember(5)] internal float EnergyCost;
            [ProtoMember(6)] internal float Strength;
        }

        [ProtoContract]
        public class UpgradeDefinition
        {
            [ProtoMember(1)] internal ModelAssignmentsDef Assignments;
            [ProtoMember(2)] internal HardPointDef HardPoint;
            [ProtoMember(3)] internal WeaponDefinition.AnimationDef Animations;
            [ProtoMember(4)] internal string ModPath;
            [ProtoMember(5)] internal ConsumeableDef[] Consumable;

            [ProtoContract]
            public struct ModelAssignmentsDef
            {
                [ProtoMember(1)] internal MountPointDef[] MountPoints;

                [ProtoContract]
                public struct MountPointDef
                {
                    [ProtoMember(1)] internal string SubtypeId;
                    [ProtoMember(2)] internal float DurabilityMod;
                    [ProtoMember(3)] internal string IconName;
                }
            }

            [ProtoContract]
            public struct HardPointDef
            {
                [ProtoMember(1)] internal string PartName;
                [ProtoMember(2)] internal HardwareDef HardWare;
                [ProtoMember(3)] internal UiDef Ui;
                [ProtoMember(4)] internal OtherDef Other;

                [ProtoContract]
                public struct UiDef
                {
                    [ProtoMember(1)] internal bool StrengthModifier;
                }

                [ProtoContract]
                public struct HardwareDef
                {
                    public enum HardwareType
                    {
                        Default,
                    }

                    [ProtoMember(1)] internal float InventorySize;
                    [ProtoMember(2)] internal HardwareType Type;
                    [ProtoMember(3)] internal int BlockDistance;

                }

                [ProtoContract]
                public struct OtherDef
                {
                    [ProtoMember(1)] internal int ConstructPartCap;
                    [ProtoMember(2)] internal int EnergyPriority;
                    [ProtoMember(3)] internal bool Debug;
                    [ProtoMember(4)] internal double RestrictionRadius;
                    [ProtoMember(5)] internal bool CheckInflatedBox;
                    [ProtoMember(6)] internal bool CheckForAnySupport;
                    [ProtoMember(7)] internal bool StayCharged;
                }
            }
        }

        [ProtoContract]
        public class SupportDefinition
        {
            [ProtoMember(1)] internal ModelAssignmentsDef Assignments;
            [ProtoMember(2)] internal HardPointDef HardPoint;
            [ProtoMember(3)] internal WeaponDefinition.AnimationDef Animations;
            [ProtoMember(4)] internal string ModPath;
            [ProtoMember(5)] internal ConsumeableDef[] Consumable;
            [ProtoMember(6)] internal SupportEffect Effect;

            [ProtoContract]
            public struct ModelAssignmentsDef
            {
                [ProtoMember(1)] internal MountPointDef[] MountPoints;

                [ProtoContract]
                public struct MountPointDef
                {
                    [ProtoMember(1)] internal string SubtypeId;
                    [ProtoMember(2)] internal float DurabilityMod;
                    [ProtoMember(3)] internal string IconName;
                }
            }
            [ProtoContract]
            public struct HardPointDef
            {
                [ProtoMember(1)] internal string PartName;
                [ProtoMember(2)] internal HardwareDef HardWare;
                [ProtoMember(3)] internal UiDef Ui;
                [ProtoMember(4)] internal OtherDef Other;

                [ProtoContract]
                public struct UiDef
                {
                    [ProtoMember(1)] internal bool ProtectionControl;
                }

                [ProtoContract]
                public struct HardwareDef
                {
                    [ProtoMember(1)] internal float InventorySize;
                }

                [ProtoContract]
                public struct OtherDef
                {
                    [ProtoMember(1)] internal int ConstructPartCap;
                    [ProtoMember(2)] internal int EnergyPriority;
                    [ProtoMember(3)] internal bool Debug;
                    [ProtoMember(4)] internal double RestrictionRadius;
                    [ProtoMember(5)] internal bool CheckInflatedBox;
                    [ProtoMember(6)] internal bool CheckForAnySupport;
                    [ProtoMember(7)] internal bool StayCharged;
                }
            }

            [ProtoContract]
            public struct SupportEffect
            {
                public enum AffectedBlocks
                {
                    Armor,
                    ArmorPlus,
                    PlusFunctional,
                    All,
                }

                public enum Protections
                {
                    KineticProt,
                    EnergeticProt,
                    GenericProt,
                    Regenerate,
                    Structural,
                }

                [ProtoMember(1)] internal Protections Protection;
                [ProtoMember(2)] internal AffectedBlocks Affected;
                [ProtoMember(3)] internal int BlockRange;
                [ProtoMember(4)] internal int MaxPoints;
                [ProtoMember(5)] internal int PointsPerCharge;
                [ProtoMember(6)] internal int UsablePerSecond;
                [ProtoMember(7)] internal int UsablePerMinute;
                [ProtoMember(8)] internal float Overflow;
                [ProtoMember(9)] internal float Effectiveness;
                [ProtoMember(10)] internal float ProtectionMin;
                [ProtoMember(11)] internal float ProtectionMax;
            }
        }

        [ProtoContract]
        public class ArmorDefinition
        {
            internal enum ArmorType
            {
                Light,
                Heavy,
                NonArmor,
            }

            [ProtoMember(1)] internal string[] SubtypeIds;
            [ProtoMember(2)] internal ArmorType Kind;
            [ProtoMember(3)] internal double KineticResistance;
            [ProtoMember(4)] internal double EnergeticResistance;
        }

        [ProtoContract]
        public class WeaponDefinition
        {
            [ProtoMember(1)] internal ModelAssignmentsDef Assignments;
            [ProtoMember(2)] internal TargetingDef Targeting;
            [ProtoMember(3)] internal AnimationDef Animations;
            [ProtoMember(4)] internal HardPointDef HardPoint;
            [ProtoMember(5)] internal AmmoDef[] Ammos;
            [ProtoMember(6)] internal string ModPath;
            [ProtoMember(7)] internal Dictionary<string, UpgradeValues[]> Upgrades;

            [ProtoContract]
            public struct ModelAssignmentsDef
            {
                [ProtoMember(1)] internal MountPointDef[] MountPoints;
                [ProtoMember(2)] internal string[] Muzzles;
                [ProtoMember(3)] internal string Ejector;
                [ProtoMember(4)] internal string Scope;

                [ProtoContract]
                public struct MountPointDef
                {
                    [ProtoMember(1)] internal string SubtypeId;
                    [ProtoMember(2)] internal string SpinPartId;
                    [ProtoMember(3)] internal string MuzzlePartId;
                    [ProtoMember(4)] internal string AzimuthPartId;
                    [ProtoMember(5)] internal string ElevationPartId;
                    [ProtoMember(6)] internal float DurabilityMod;
                    [ProtoMember(7)] internal string IconName;
                }
            }

            [ProtoContract]
            public struct TargetingDef
            {
                public enum Threat
                {
                    Projectiles,
                    Characters,
                    Grids,
                    Neutrals,
                    Meteors,
                    Other,
                    ScanNeutralGrid,
                    ScanFriendlyGrid,
                    ScanFriendlyCharacter,
                    ScanRoid,
                    ScanPlanet,
                    ScanEnemyCharacter,
                    ScanEnemyGrid,
                    ScanNeutralCharacter,
                    ScanUnOwnedGrid,
                    ScanOwnersGrid
                }

                public enum BlockTypes
                {
                    Any,
                    Offense,
                    Utility,
                    Power,
                    Production,
                    Thrust,
                    Jumping,
                    Steering
                }

                [ProtoMember(1)] internal int TopTargets;
                [ProtoMember(2)] internal int TopBlocks;
                [ProtoMember(3)] internal double StopTrackingSpeed;
                [ProtoMember(4)] internal float MinimumDiameter;
                [ProtoMember(5)] internal float MaximumDiameter;
                [ProtoMember(6)] internal bool ClosestFirst;
                [ProtoMember(7)] internal BlockTypes[] SubSystems;
                [ProtoMember(8)] internal Threat[] Threats;
                [ProtoMember(9)] internal float MaxTargetDistance;
                [ProtoMember(10)] internal float MinTargetDistance;
                [ProtoMember(11)] internal bool IgnoreDumbProjectiles;
                [ProtoMember(12)] internal bool LockedSmartOnly;
                [ProtoMember(13)] internal bool UniqueTargetPerWeapon;
                [ProtoMember(14)] internal int MaxTrackingTime;
                [ProtoMember(15)] internal bool ShootBlanks;
                [ProtoMember(19)] internal CommunicationDef Communications;
                [ProtoMember(20)] internal bool FocusOnly;
                [ProtoMember(21)] internal bool EvictUniqueTargets;
                [ProtoMember(22)] internal int CycleTargets;
                [ProtoMember(23)] internal int CycleBlocks;

                [ProtoContract]
                public struct CommunicationDef
                {
                    public enum Comms
                    {
                        NoComms,
                        BroadCast,
                        Relay,
                        Jamming,
                        RelayAndBroadCast,
                    }

                    public enum SecurityMode
                    {
                        Public,
                        Private,
                        Secure,
                    }

                    [ProtoMember(1)] internal bool StoreTargets;
                    [ProtoMember(2)] internal int StorageLimit;
                    [ProtoMember(3)] internal string StorageLocation;
                    [ProtoMember(4)] internal Comms Mode;
                    [ProtoMember(5)] internal SecurityMode Security;
                    [ProtoMember(6)] internal string BroadCastChannel;
                    [ProtoMember(7)] internal double BroadCastRange;
                    [ProtoMember(8)] internal double JammingStrength;
                    [ProtoMember(9)] internal string RelayChannel;
                    [ProtoMember(10)] internal double RelayRange;
                    [ProtoMember(11)] internal bool TargetPersists;
                    [ProtoMember(12)] internal bool StoreLimitPerBlock;
                    [ProtoMember(13)] internal int MaxConnections;
                }
            }


            [ProtoContract]
            public struct AnimationDef
            {
                [ProtoMember(1)] internal PartAnimationSetDef[] AnimationSets;
                [ProtoMember(2)] internal PartEmissive[] Emissives;
                [ProtoMember(3)] internal string[] HeatingEmissiveParts;
                [ProtoMember(4)] internal Dictionary<PartAnimationSetDef.EventTriggers, EventParticle[]> EventParticles;

                [ProtoContract(IgnoreListHandling = true)]
                public struct PartAnimationSetDef
                {
                    public enum EventTriggers
                    {
                        Reloading,
                        Firing,
                        Tracking,
                        Overheated,
                        TurnOn,
                        TurnOff,
                        BurstReload,
                        NoMagsToLoad,
                        PreFire,
                        EmptyOnGameLoad,
                        StopFiring,
                        StopTracking,
                        LockDelay,
                    }

                    public enum ResetConditions
                    {
                        None,
                        Home,
                        Off,
                        On,
                        Reloaded
                    }

                    [ProtoMember(1)] internal string[] SubpartId;
                    [ProtoMember(2)] internal string BarrelId;
                    [ProtoMember(3)] internal uint StartupFireDelay;
                    [ProtoMember(4)] internal Dictionary<EventTriggers, uint> AnimationDelays;
                    [ProtoMember(5)] internal EventTriggers[] Reverse;
                    [ProtoMember(6)] internal EventTriggers[] Loop;
                    [ProtoMember(7)] internal Dictionary<EventTriggers, RelMove[]> EventMoveSets;
                    [ProtoMember(8)] internal EventTriggers[] TriggerOnce;
                    [ProtoMember(9)] internal EventTriggers[] ResetEmissives;
                    [ProtoMember(10)] internal ResetConditions Resets;

                }

                [ProtoContract]
                public struct PartEmissive
                {
                    [ProtoMember(1)] internal string EmissiveName;
                    [ProtoMember(2)] internal string[] EmissivePartNames;
                    [ProtoMember(3)] internal bool CycleEmissivesParts;
                    [ProtoMember(4)] internal bool LeavePreviousOn;
                    [ProtoMember(5)] internal Vector4[] Colors;
                    [ProtoMember(6)] internal float[] IntensityRange;
                }
                [ProtoContract]
                public struct EventParticle
                {
                    [ProtoMember(1)] internal string[] EmptyNames;
                    [ProtoMember(2)] internal string[] MuzzleNames;
                    [ProtoMember(3)] internal ParticleDef Particle;
                    [ProtoMember(4)] internal uint StartDelay;
                    [ProtoMember(5)] internal uint LoopDelay;
                    [ProtoMember(6)] internal bool ForceStop;
                }
                [ProtoContract]
                internal struct RelMove
                {
                    public enum MoveType
                    {
                        Linear,
                        ExpoDecay,
                        ExpoGrowth,
                        Delay,
                        Show, //instant or fade
                        Hide, //instant or fade
                    }

                    [ProtoMember(1)] internal MoveType MovementType;
                    [ProtoMember(2)] internal XYZ[] LinearPoints;
                    [ProtoMember(3)] internal XYZ Rotation;
                    [ProtoMember(4)] internal XYZ RotAroundCenter;
                    [ProtoMember(5)] internal uint TicksToMove;
                    [ProtoMember(6)] internal string CenterEmpty;
                    [ProtoMember(7)] internal bool Fade;
                    [ProtoMember(8)] internal string EmissiveName;

                    [ProtoContract]
                    internal struct XYZ
                    {
                        [ProtoMember(1)] internal double x;
                        [ProtoMember(2)] internal double y;
                        [ProtoMember(3)] internal double z;
                    }
                }
            }

            [ProtoContract]
            public struct UpgradeValues
            {
                [ProtoMember(1)] internal string[] Ammo;
                [ProtoMember(2)] internal Dependency[] Dependencies;
                [ProtoMember(3)] internal int RateOfFireMod;
                [ProtoMember(4)] internal int BarrelsPerShotMod;
                [ProtoMember(5)] internal int ReloadMod;
                [ProtoMember(6)] internal int MaxHeatMod;
                [ProtoMember(7)] internal int HeatSinkRateMod;
                [ProtoMember(8)] internal int ShotsInBurstMod;
                [ProtoMember(9)] internal int DelayAfterBurstMod;
                [ProtoMember(10)] internal int AmmoPriority;

                [ProtoContract]
                public struct Dependency
                {
                    internal string SubtypeId;
                    internal int Quanity;
                }
            }

            [ProtoContract]
            public struct HardPointDef
            {
                public enum Prediction
                {
                    Off,
                    Basic,
                    Accurate,
                    Advanced,
                }

                [ProtoMember(1)] internal string PartName;
                [ProtoMember(2)] internal int DelayCeaseFire;
                [ProtoMember(3)] internal float DeviateShotAngle;
                [ProtoMember(4)] internal double AimingTolerance;
                [ProtoMember(5)] internal Prediction AimLeadingPrediction;
                [ProtoMember(6)] internal LoadingDef Loading;
                [ProtoMember(7)] internal AiDef Ai;
                [ProtoMember(8)] internal HardwareDef HardWare;
                [ProtoMember(9)] internal UiDef Ui;
                [ProtoMember(10)] internal HardPointAudioDef Audio;
                [ProtoMember(11)] internal HardPointParticleDef Graphics;
                [ProtoMember(12)] internal OtherDef Other;
                [ProtoMember(13)] internal bool AddToleranceToTracking;
                [ProtoMember(14)] internal bool CanShootSubmerged;
                [ProtoMember(15)] internal bool NpcSafe;
                [ProtoMember(16)] internal bool ScanTrackOnly;

                [ProtoContract]
                public struct LoadingDef
                {
                    [ProtoMember(1)] internal int ReloadTime;
                    [ProtoMember(2)] internal int RateOfFire;
                    [ProtoMember(3)] internal int BarrelsPerShot;
                    [ProtoMember(4)] internal int SkipBarrels;
                    [ProtoMember(5)] internal int TrajectilesPerBarrel;
                    [ProtoMember(6)] internal int HeatPerShot;
                    [ProtoMember(7)] internal int MaxHeat;
                    [ProtoMember(8)] internal int HeatSinkRate;
                    [ProtoMember(9)] internal float Cooldown;
                    [ProtoMember(10)] internal int DelayUntilFire;
                    [ProtoMember(11)] internal int ShotsInBurst;
                    [ProtoMember(12)] internal int DelayAfterBurst;
                    [ProtoMember(13)] internal bool DegradeRof;
                    [ProtoMember(14)] internal int BarrelSpinRate;
                    [ProtoMember(15)] internal bool FireFull;
                    [ProtoMember(16)] internal bool GiveUpAfter;
                    [ProtoMember(17)] internal bool DeterministicSpin;
                    [ProtoMember(18)] internal bool SpinFree;
                    [ProtoMember(19)] internal bool StayCharged;
                    [ProtoMember(20)] internal int MagsToLoad;
                    [ProtoMember(21)] internal int MaxActiveProjectiles;
                    [ProtoMember(22)] internal int MaxReloads;
                    [ProtoMember(23)] internal bool GoHomeToReload;
                    [ProtoMember(24)] internal bool DropTargetUntilLoaded;
                }


                [ProtoContract]
                public struct UiDef
                {
                    [ProtoMember(1)] internal bool RateOfFire;
                    [ProtoMember(2)] internal bool DamageModifier;
                    [ProtoMember(3)] internal bool ToggleGuidance;
                    [ProtoMember(4)] internal bool EnableOverload;
                    [ProtoMember(5)] internal bool AlternateUi;
                    [ProtoMember(6)] internal bool DisableStatus;
                }


                [ProtoContract]
                public struct AiDef
                {
                    [ProtoMember(1)] internal bool TrackTargets;
                    [ProtoMember(2)] internal bool TurretAttached;
                    [ProtoMember(3)] internal bool TurretController;
                    [ProtoMember(4)] internal bool PrimaryTracking;
                    [ProtoMember(5)] internal bool LockOnFocus;
                    [ProtoMember(6)] internal bool SuppressFire;
                    [ProtoMember(7)] internal bool OverrideLeads;
                    [ProtoMember(8)] internal int DefaultLeadGroup;
                    [ProtoMember(9)] internal bool TargetGridCenter;
                }

                [ProtoContract]
                public struct HardwareDef
                {
                    public enum HardwareType
                    {
                        BlockWeapon = 0,
                        HandWeapon = 1,
                        Phantom = 6,
                    }

                    [ProtoMember(1)] internal float RotateRate;
                    [ProtoMember(2)] internal float ElevateRate;
                    [ProtoMember(3)] internal Vector3D Offset;
                    [ProtoMember(4)] internal bool FixedOffset;
                    [ProtoMember(5)] internal int MaxAzimuth;
                    [ProtoMember(6)] internal int MinAzimuth;
                    [ProtoMember(7)] internal int MaxElevation;
                    [ProtoMember(8)] internal int MinElevation;
                    [ProtoMember(9)] internal float InventorySize;
                    [ProtoMember(10)] internal HardwareType Type;
                    [ProtoMember(11)] internal int HomeAzimuth;
                    [ProtoMember(12)] internal int HomeElevation;
                    [ProtoMember(13)] internal CriticalDef CriticalReaction;
                    [ProtoMember(14)] internal float IdlePower;

                    [ProtoContract]
                    public struct CriticalDef
                    {
                        [ProtoMember(1)] internal bool Enable;
                        [ProtoMember(2)] internal int DefaultArmedTimer;
                        [ProtoMember(3)] internal bool PreArmed;
                        [ProtoMember(4)] internal bool TerminalControls;
                        [ProtoMember(5)] internal string AmmoRound;
                    }
                }

                [ProtoContract]
                public struct HardPointAudioDef
                {
                    [ProtoMember(1)] internal string ReloadSound;
                    [ProtoMember(2)] internal string NoAmmoSound;
                    [ProtoMember(3)] internal string HardPointRotationSound;
                    [ProtoMember(4)] internal string BarrelRotationSound;
                    [ProtoMember(5)] internal string FiringSound;
                    [ProtoMember(6)] internal bool FiringSoundPerShot;
                    [ProtoMember(7)] internal string PreFiringSound;
                    [ProtoMember(8)] internal uint FireSoundEndDelay;
                    [ProtoMember(9)] internal bool FireSoundNoBurst;
                }

                [ProtoContract]
                public struct OtherDef
                {
                    [ProtoMember(1)] internal int ConstructPartCap;
                    [ProtoMember(2)] internal int EnergyPriority;
                    [ProtoMember(3)] internal int RotateBarrelAxis;
                    [ProtoMember(4)] internal bool MuzzleCheck;
                    [ProtoMember(5)] internal bool Debug;
                    [ProtoMember(6)] internal double RestrictionRadius;
                    [ProtoMember(7)] internal bool CheckInflatedBox;
                    [ProtoMember(8)] internal bool CheckForAnyWeapon;
                    [ProtoMember(9)] internal bool DisableLosCheck;
                    [ProtoMember(10)] internal bool NoVoxelLosCheck;
                }

                [ProtoContract]
                public struct HardPointParticleDef
                {
                    [ProtoMember(1)] internal ParticleDef Effect1;
                    [ProtoMember(2)] internal ParticleDef Effect2;
                }
            }

            [ProtoContract]
            public class AmmoDef
            {
                [ProtoMember(1)] internal string AmmoMagazine;
                [ProtoMember(2)] internal string AmmoRound;
                [ProtoMember(3)] internal bool HybridRound;
                [ProtoMember(4)] internal float EnergyCost;
                [ProtoMember(5)] internal float BaseDamage;
                [ProtoMember(6)] internal float Mass;
                [ProtoMember(7)] internal float Health;
                [ProtoMember(8)] internal float BackKickForce;
                [ProtoMember(9)] internal DamageScaleDef DamageScales;
                [ProtoMember(10)] internal ShapeDef Shape;
                [ProtoMember(11)] internal ObjectsHitDef ObjectsHit;
                [ProtoMember(12)] internal TrajectoryDef Trajectory;
                [ProtoMember(13)] internal AreaDamageDef AreaEffect;
                [ProtoMember(14)] internal BeamDef Beams;
                [ProtoMember(15)] internal FragmentDef Fragment;
                [ProtoMember(16)] internal GraphicDef AmmoGraphics;
                [ProtoMember(17)] internal AmmoAudioDef AmmoAudio;
                [ProtoMember(18)] internal bool HardPointUsable;
                [ProtoMember(19)] internal PatternDef Pattern;
                [ProtoMember(20)] internal int EnergyMagazineSize;
                [ProtoMember(21)] internal float DecayPerShot;
                [ProtoMember(22)] internal EjectionDef Ejection;
                [ProtoMember(23)] internal bool IgnoreWater;
                [ProtoMember(24)] internal AreaOfDamageDef AreaOfDamage;
                [ProtoMember(25)] internal EwarDef Ewar;
                [ProtoMember(26)] internal bool IgnoreVoxels;
                [ProtoMember(27)] internal bool Synchronize;
                [ProtoMember(28)] internal double HeatModifier;
                [ProtoMember(29)] internal bool NpcSafe;
                [ProtoMember(30)] internal SynchronizeDef Sync;
                [ProtoMember(31)] internal bool NoGridOrArmorScaling;

                [ProtoContract]
                public struct SynchronizeDef
                {
                    [ProtoMember(1)] internal bool Full;
                    [ProtoMember(2)] internal bool PointDefense;
                    [ProtoMember(3)] internal bool OnHitDeath;
                }

                [ProtoContract]
                public struct DamageScaleDef
                {

                    [ProtoMember(1)] internal float MaxIntegrity;
                    [ProtoMember(2)] internal bool DamageVoxels;
                    [ProtoMember(3)] internal float Characters;
                    [ProtoMember(4)] internal bool SelfDamage;
                    [ProtoMember(5)] internal GridSizeDef Grids;
                    [ProtoMember(6)] internal ArmorDef Armor;
                    [ProtoMember(7)] internal CustomScalesDef Custom;
                    [ProtoMember(8)] internal ShieldDef Shields;
                    [ProtoMember(9)] internal FallOffDef FallOff;
                    [ProtoMember(10)] internal double HealthHitModifier;
                    [ProtoMember(11)] internal double VoxelHitModifier;
                    [ProtoMember(12)] internal DamageTypes DamageType;
                    [ProtoMember(13)] internal DeformDef Deform;

                    [ProtoContract]
                    public struct FallOffDef
                    {
                        [ProtoMember(1)] internal float Distance;
                        [ProtoMember(2)] internal float MinMultipler;
                    }

                    [ProtoContract]
                    public struct GridSizeDef
                    {
                        [ProtoMember(1)] internal float Large;
                        [ProtoMember(2)] internal float Small;
                    }

                    [ProtoContract]
                    public struct ArmorDef
                    {
                        [ProtoMember(1)] internal float Armor;
                        [ProtoMember(2)] internal float Heavy;
                        [ProtoMember(3)] internal float Light;
                        [ProtoMember(4)] internal float NonArmor;
                    }

                    [ProtoContract]
                    public struct CustomScalesDef
                    {
                        internal enum SkipMode
                        {
                            NoSkip,
                            Inclusive,
                            Exclusive,
                        }

                        [ProtoMember(1)] internal CustomBlocksDef[] Types;
                        [ProtoMember(2)] internal bool IgnoreAllOthers;
                        [ProtoMember(3)] internal SkipMode SkipOthers;
                    }

                    [ProtoContract]
                    public struct DamageTypes
                    {
                        internal enum Damage
                        {
                            Energy,
                            Kinetic,
                        }

                        [ProtoMember(1)] internal Damage Base;
                        [ProtoMember(2)] internal Damage AreaEffect;
                        [ProtoMember(3)] internal Damage Detonation;
                        [ProtoMember(4)] internal Damage Shield;
                    }

                    [ProtoContract]
                    public struct ShieldDef
                    {
                        internal enum ShieldType
                        {
                            Default,
                            Heal,
                            Bypass,
                            EmpRetired,
                        }

                        [ProtoMember(1)] internal float Modifier;
                        [ProtoMember(2)] internal ShieldType Type;
                        [ProtoMember(3)] internal float BypassModifier;
                    }

                    [ProtoContract]
                    public struct DeformDef
                    {
                        internal enum DeformTypes
                        {
                            HitBlock,
                            AllDamagedBlocks,
                            NoDeform,
                        }

                        [ProtoMember(1)] internal DeformTypes DeformType;
                        [ProtoMember(2)] internal int DeformDelay;
                    }
                }

                [ProtoContract]
                public struct ShapeDef
                {
                    public enum Shapes
                    {
                        LineShape,
                        SphereShape,
                    }

                    [ProtoMember(1)] internal Shapes Shape;
                    [ProtoMember(2)] internal double Diameter;
                }

                [ProtoContract]
                public struct ObjectsHitDef
                {
                    [ProtoMember(1)] internal int MaxObjectsHit;
                    [ProtoMember(2)] internal bool CountBlocks;
                }


                [ProtoContract]
                public struct CustomBlocksDef
                {
                    [ProtoMember(1)] internal string SubTypeId;
                    [ProtoMember(2)] internal float Modifier;
                }

                [ProtoContract]
                public struct GraphicDef
                {
                    [ProtoMember(1)] internal bool ShieldHitDraw;
                    [ProtoMember(2)] internal float VisualProbability;
                    [ProtoMember(3)] internal string ModelName;
                    [ProtoMember(4)] internal AmmoParticleDef Particles;
                    [ProtoMember(5)] internal LineDef Lines;
                    [ProtoMember(6)] internal DecalDef Decals;

                    [ProtoContract]
                    public struct AmmoParticleDef
                    {
                        [ProtoMember(1)] internal ParticleDef Ammo;
                        [ProtoMember(2)] internal ParticleDef Hit;
                        [ProtoMember(3)] internal ParticleDef Eject;
                    }

                    [ProtoContract]
                    public struct LineDef
                    {
                        internal enum Texture
                        {
                            Normal,
                            Cycle,
                            Chaos,
                            Wave,
                        }
                        public enum FactionColor
                        {
                            DontUse,
                            Foreground,
                            Background,
                        }

                        [ProtoMember(1)] internal TracerBaseDef Tracer;
                        [ProtoMember(2)] internal string TracerMaterial;
                        [ProtoMember(3)] internal Randomize ColorVariance;
                        [ProtoMember(4)] internal Randomize WidthVariance;
                        [ProtoMember(5)] internal TrailDef Trail;
                        [ProtoMember(6)] internal OffsetEffectDef OffsetEffect;
                        [ProtoMember(7)] internal bool DropParentVelocity;

                        [ProtoContract]
                        public struct OffsetEffectDef
                        {
                            [ProtoMember(1)] internal double MaxOffset;
                            [ProtoMember(2)] internal double MinLength;
                            [ProtoMember(3)] internal double MaxLength;
                        }

                        [ProtoContract]
                        public struct TracerBaseDef
                        {
                            [ProtoMember(1)] internal bool Enable;
                            [ProtoMember(2)] internal float Length;
                            [ProtoMember(3)] internal float Width;
                            [ProtoMember(4)] internal Vector4 Color;
                            [ProtoMember(5)] internal uint VisualFadeStart;
                            [ProtoMember(6)] internal uint VisualFadeEnd;
                            [ProtoMember(7)] internal SegmentDef Segmentation;
                            [ProtoMember(8)] internal string[] Textures;
                            [ProtoMember(9)] internal Texture TextureMode;
                            [ProtoMember(10)] internal bool AlwaysDraw;
                            [ProtoMember(11)] internal FactionColor FactionColor;

                            [ProtoContract]
                            public struct SegmentDef
                            {
                                [ProtoMember(1)] internal string Material; //retired
                                [ProtoMember(2)] internal double SegmentLength;
                                [ProtoMember(3)] internal double SegmentGap;
                                [ProtoMember(4)] internal double Speed;
                                [ProtoMember(5)] internal Vector4 Color;
                                [ProtoMember(6)] internal double WidthMultiplier;
                                [ProtoMember(7)] internal bool Reverse;
                                [ProtoMember(8)] internal bool UseLineVariance;
                                [ProtoMember(9)] internal Randomize ColorVariance;
                                [ProtoMember(10)] internal Randomize WidthVariance;
                                [ProtoMember(11)] internal string[] Textures;
                                [ProtoMember(12)] internal bool Enable;
                                [ProtoMember(13)] internal FactionColor FactionColor;
                            }
                        }

                        [ProtoContract]
                        public struct TrailDef
                        {
                            [ProtoMember(1)] internal bool Enable;
                            [ProtoMember(2)] internal string Material;
                            [ProtoMember(3)] internal int DecayTime;
                            [ProtoMember(4)] internal Vector4 Color;
                            [ProtoMember(5)] internal bool Back;
                            [ProtoMember(6)] internal float CustomWidth;
                            [ProtoMember(7)] internal bool UseWidthVariance;
                            [ProtoMember(8)] internal bool UseColorFade;
                            [ProtoMember(9)] internal string[] Textures;
                            [ProtoMember(10)] internal Texture TextureMode;
                            [ProtoMember(11)] internal bool AlwaysDraw;
                            [ProtoMember(12)] internal FactionColor FactionColor;
                        }
                    }

                    [ProtoContract]
                    public struct DecalDef
                    {

                        [ProtoMember(1)] internal int MaxAge;
                        [ProtoMember(2)] internal TextureMapDef[] Map;

                        [ProtoContract]
                        public struct TextureMapDef
                        {
                            [ProtoMember(1)] internal string HitMaterial;
                            [ProtoMember(2)] internal string DecalMaterial;
                        }
                    }
                }

                [ProtoContract]
                public struct BeamDef
                {
                    [ProtoMember(1)] internal bool Enable;
                    [ProtoMember(2)] internal bool ConvergeBeams;
                    [ProtoMember(3)] internal bool VirtualBeams;
                    [ProtoMember(4)] internal bool RotateRealBeam;
                    [ProtoMember(5)] internal bool OneParticle;
                    [ProtoMember(6)] internal bool FakeVoxelHits;
                }

                [ProtoContract]
                public struct FragmentDef
                {
                    [ProtoMember(1)] internal string AmmoRound;
                    [ProtoMember(2)] internal int Fragments;
                    [ProtoMember(3)] internal float Radial;
                    [ProtoMember(4)] internal float BackwardDegrees;
                    [ProtoMember(5)] internal float Degrees;
                    [ProtoMember(6)] internal bool Reverse;
                    [ProtoMember(7)] internal bool IgnoreArming;
                    [ProtoMember(8)] internal bool DropVelocity;
                    [ProtoMember(9)] internal float Offset;
                    [ProtoMember(10)] internal int MaxChildren;
                    [ProtoMember(11)] internal TimedSpawnDef TimedSpawns;
                    [ProtoMember(12)] internal bool FireSound;
                    [ProtoMember(13)] internal Vector3D AdvOffset;
                    [ProtoMember(14)] internal bool ArmWhenHit;

                    [ProtoContract]
                    public struct TimedSpawnDef
                    {
                        public enum PointTypes
                        {
                            Direct,
                            Lead,
                            Predict,
                        }

                        [ProtoMember(1)] internal bool Enable;
                        [ProtoMember(2)] internal int Interval;
                        [ProtoMember(3)] internal int StartTime;
                        [ProtoMember(4)] internal int MaxSpawns;
                        [ProtoMember(5)] internal double Proximity;
                        [ProtoMember(6)] internal bool ParentDies;
                        [ProtoMember(7)] internal bool PointAtTarget;
                        [ProtoMember(8)] internal int GroupSize;
                        [ProtoMember(9)] internal int GroupDelay;
                        [ProtoMember(10)] internal PointTypes PointType;
                    }
                }

                [ProtoContract]
                public struct PatternDef
                {
                    public enum PatternModes
                    {
                        Never,
                        Weapon,
                        Fragment,
                        Both,
                    }


                    [ProtoMember(1)] internal string[] Patterns;
                    [ProtoMember(2)] internal bool Enable;
                    [ProtoMember(3)] internal float TriggerChance;
                    [ProtoMember(4)] internal bool SkipParent;
                    [ProtoMember(5)] internal bool Random;
                    [ProtoMember(6)] internal int RandomMin;
                    [ProtoMember(7)] internal int RandomMax;
                    [ProtoMember(8)] internal int PatternSteps;
                    [ProtoMember(9)] internal PatternModes Mode;
                }

                [ProtoContract]
                public struct EjectionDef
                {
                    public enum SpawnType
                    {
                        Item,
                        Particle,
                    }
                    [ProtoMember(1)] internal float Speed;
                    [ProtoMember(2)] internal float SpawnChance;
                    [ProtoMember(3)] internal SpawnType Type;
                    [ProtoMember(4)] internal ComponentDef CompDef;

                    [ProtoContract]
                    public struct ComponentDef
                    {
                        [ProtoMember(1)] internal string ItemName;
                        [ProtoMember(2)] internal int ItemLifeTime;
                        [ProtoMember(3)] internal int Delay;
                    }
                }

                [ProtoContract]
                public struct AreaOfDamageDef
                {
                    public enum Falloff
                    {
                        Legacy,
                        NoFalloff,
                        Linear,
                        Curve,
                        InvCurve,
                        Squeeze,
                        Pooled,
                        Exponential,
                    }

                    public enum AoeShape
                    {
                        Round,
                        Diamond,
                    }

                    [ProtoMember(1)] internal ByBlockHitDef ByBlockHit;
                    [ProtoMember(2)] internal EndOfLifeDef EndOfLife;

                    [ProtoContract]
                    public struct ByBlockHitDef
                    {
                        [ProtoMember(1)] internal bool Enable;
                        [ProtoMember(2)] internal double Radius;
                        [ProtoMember(3)] internal float Damage;
                        [ProtoMember(4)] internal float Depth;
                        [ProtoMember(5)] internal float MaxAbsorb;
                        [ProtoMember(6)] internal Falloff Falloff;
                        [ProtoMember(7)] internal AoeShape Shape;
                    }

                    [ProtoContract]
                    public struct EndOfLifeDef
                    {
                        [ProtoMember(1)] internal bool Enable;
                        [ProtoMember(2)] internal double Radius;
                        [ProtoMember(3)] internal float Damage;
                        [ProtoMember(4)] internal float Depth;
                        [ProtoMember(5)] internal float MaxAbsorb;
                        [ProtoMember(6)] internal Falloff Falloff;
                        [ProtoMember(7)] internal bool ArmOnlyOnHit;
                        [ProtoMember(8)] internal int MinArmingTime;
                        [ProtoMember(9)] internal bool NoVisuals;
                        [ProtoMember(10)] internal bool NoSound;
                        [ProtoMember(11)] internal float ParticleScale;
                        [ProtoMember(12)] internal string CustomParticle;
                        [ProtoMember(13)] internal string CustomSound;
                        [ProtoMember(14)] internal AoeShape Shape;
                    }
                }

                [ProtoContract]
                public struct EwarDef
                {
                    public enum EwarType
                    {
                        AntiSmart,
                        JumpNull,
                        EnergySink,
                        Anchor,
                        Emp,
                        Offense,
                        Nav,
                        Dot,
                        Push,
                        Pull,
                        Tractor,
                    }

                    public enum EwarMode
                    {
                        Effect,
                        Field,
                    }

                    [ProtoMember(1)] internal bool Enable;
                    [ProtoMember(2)] internal EwarType Type;
                    [ProtoMember(3)] internal EwarMode Mode;
                    [ProtoMember(4)] internal float Strength;
                    [ProtoMember(5)] internal double Radius;
                    [ProtoMember(6)] internal int Duration;
                    [ProtoMember(7)] internal bool StackDuration;
                    [ProtoMember(8)] internal bool Depletable;
                    [ProtoMember(9)] internal int MaxStacks;
                    [ProtoMember(10)] internal bool NoHitParticle;
                    [ProtoMember(11)] internal PushPullDef Force;
                    [ProtoMember(12)] internal FieldDef Field;


                    [ProtoContract]
                    public struct FieldDef
                    {
                        [ProtoMember(1)] internal int Interval;
                        [ProtoMember(2)] internal int PulseChance;
                        [ProtoMember(3)] internal int GrowTime;
                        [ProtoMember(4)] internal bool HideModel;
                        [ProtoMember(5)] internal bool ShowParticle;
                        [ProtoMember(6)] internal double TriggerRange;
                        [ProtoMember(7)] internal ParticleDef Particle;
                    }

                    [ProtoContract]
                    public struct PushPullDef
                    {
                        public enum Force
                        {
                            ProjectileLastPosition,
                            ProjectileOrigin,
                            HitPosition,
                            TargetCenter,
                            TargetCenterOfMass,
                        }

                        [ProtoMember(1)] internal Force ForceFrom;
                        [ProtoMember(2)] internal Force ForceTo;
                        [ProtoMember(3)] internal Force Position;
                        [ProtoMember(4)] internal bool DisableRelativeMass;
                        [ProtoMember(5)] internal double TractorRange;
                        [ProtoMember(6)] internal bool ShooterFeelsForce;
                    }
                }


                [ProtoContract]
                public struct AreaDamageDef
                {
                    public enum AreaEffectType
                    {
                        Disabled,
                        Explosive,
                        Radiant,
                        AntiSmart,
                        JumpNullField,
                        EnergySinkField,
                        AnchorField,
                        EmpField,
                        OffenseField,
                        NavField,
                        DotField,
                        PushField,
                        PullField,
                        TractorField,
                    }

                    [ProtoMember(1)] internal double AreaEffectRadius;
                    [ProtoMember(2)] internal float AreaEffectDamage;
                    [ProtoMember(3)] internal AreaEffectType AreaEffect;
                    [ProtoMember(4)] internal PulseDef Pulse;
                    [ProtoMember(5)] internal DetonateDef Detonation;
                    [ProtoMember(6)] internal ExplosionDef Explosions;
                    [ProtoMember(7)] internal EwarFieldsDef EwarFields;
                    [ProtoMember(8)] internal AreaInfluence Base;

                    [ProtoContract]
                    public struct AreaInfluence
                    {
                        [ProtoMember(1)] internal double Radius;
                        [ProtoMember(2)] internal float EffectStrength;
                    }


                    [ProtoContract]
                    public struct PulseDef
                    {
                        [ProtoMember(1)] internal int Interval;
                        [ProtoMember(2)] internal int PulseChance;
                        [ProtoMember(3)] internal int GrowTime;
                        [ProtoMember(4)] internal bool HideModel;
                        [ProtoMember(5)] internal bool ShowParticle;
                        [ProtoMember(6)] internal ParticleDef Particle;
                    }

                    [ProtoContract]
                    public struct EwarFieldsDef
                    {
                        [ProtoMember(1)] internal int Duration;
                        [ProtoMember(2)] internal bool StackDuration;
                        [ProtoMember(3)] internal bool Depletable;
                        [ProtoMember(4)] internal double TriggerRange;
                        [ProtoMember(5)] internal int MaxStacks;
                        [ProtoMember(6)] internal PushPullDef Force;
                        [ProtoMember(7)] internal bool DisableParticleEffect;

                        [ProtoContract]
                        public struct PushPullDef
                        {
                            public enum Force
                            {
                                ProjectileLastPosition,
                                ProjectileOrigin,
                                HitPosition,
                                TargetCenter,
                                TargetCenterOfMass,
                            }

                            [ProtoMember(1)] internal Force ForceFrom;
                            [ProtoMember(2)] internal Force ForceTo;
                            [ProtoMember(3)] internal Force Position;
                            [ProtoMember(4)] internal bool DisableRelativeMass;
                            [ProtoMember(5)] internal double TractorRange;
                            [ProtoMember(6)] internal bool ShooterFeelsForce;
                        }
                    }

                    [ProtoContract]
                    public struct DetonateDef
                    {
                        [ProtoMember(1)] internal bool DetonateOnEnd;
                        [ProtoMember(2)] internal bool ArmOnlyOnHit;
                        [ProtoMember(3)] internal float DetonationRadius;
                        [ProtoMember(4)] internal float DetonationDamage;
                        [ProtoMember(5)] internal int MinArmingTime;
                    }

                    [ProtoContract]
                    public struct ExplosionDef
                    {
                        [ProtoMember(1)] internal bool NoVisuals;
                        [ProtoMember(2)] internal bool NoSound;
                        [ProtoMember(3)] internal float Scale;
                        [ProtoMember(4)] internal string CustomParticle;
                        [ProtoMember(5)] internal string CustomSound;
                        [ProtoMember(6)] internal bool NoShrapnel;
                        [ProtoMember(7)] internal bool NoDeformation;
                    }
                }

                [ProtoContract]
                public struct AmmoAudioDef
                {
                    [ProtoMember(1)] internal string TravelSound;
                    [ProtoMember(2)] internal string HitSound;
                    [ProtoMember(3)] internal float HitPlayChance;
                    [ProtoMember(4)] internal bool HitPlayShield;
                    [ProtoMember(5)] internal string VoxelHitSound;
                    [ProtoMember(6)] internal string PlayerHitSound;
                    [ProtoMember(7)] internal string FloatingHitSound;
                    [ProtoMember(8)] internal string ShieldHitSound;
                    [ProtoMember(9)] internal string ShotSound;
                }

                [ProtoContract]
                public struct TrajectoryDef
                {
                    internal enum GuidanceType
                    {
                        None,
                        Remote,
                        TravelTo,
                        Smart,
                        DetectTravelTo,
                        DetectSmart,
                        DetectFixed,
                        DroneAdvanced,
                    }

                    [ProtoMember(1)] internal float MaxTrajectory;
                    [ProtoMember(2)] internal float AccelPerSec;
                    [ProtoMember(3)] internal float DesiredSpeed;
                    [ProtoMember(4)] internal float TargetLossDegree;
                    [ProtoMember(5)] internal int TargetLossTime;
                    [ProtoMember(6)] internal int MaxLifeTime;
                    [ProtoMember(7)] internal int DeaccelTime;
                    [ProtoMember(8)] internal Randomize SpeedVariance;
                    [ProtoMember(9)] internal Randomize RangeVariance;
                    [ProtoMember(10)] internal GuidanceType Guidance;
                    [ProtoMember(11)] internal SmartsDef Smarts;
                    [ProtoMember(12)] internal MinesDef Mines;
                    [ProtoMember(13)] internal float GravityMultiplier;
                    [ProtoMember(14)] internal uint MaxTrajectoryTime;
                    [ProtoMember(15)] internal ApproachDef[] Approaches;
                    [ProtoMember(16)] internal double TotalAcceleration;

                    [ProtoContract]
                    public struct SmartsDef
                    {
                        [ProtoMember(1)] internal double Inaccuracy;
                        [ProtoMember(2)] internal double Aggressiveness;
                        [ProtoMember(3)] internal double MaxLateralThrust;
                        [ProtoMember(4)] internal double TrackingDelay;
                        [ProtoMember(5)] internal int MaxChaseTime;
                        [ProtoMember(6)] internal bool OverideTarget;
                        [ProtoMember(7)] internal int MaxTargets;
                        [ProtoMember(8)] internal bool NoTargetExpire;
                        [ProtoMember(9)] internal bool Roam;
                        [ProtoMember(10)] internal bool KeepAliveAfterTargetLoss;
                        [ProtoMember(11)] internal float OffsetRatio;
                        [ProtoMember(12)] internal int OffsetTime;
                        [ProtoMember(13)] internal bool CheckFutureIntersection;
                        [ProtoMember(14)] internal double NavAcceleration;
                        [ProtoMember(15)] internal bool AccelClearance;
                        [ProtoMember(16)] internal double SteeringLimit;
                        [ProtoMember(17)] internal bool FocusOnly;
                        [ProtoMember(18)] internal double OffsetMinRange;
                        [ProtoMember(19)] internal bool FocusEviction;
                        [ProtoMember(20)] internal double ScanRange;
                        [ProtoMember(21)] internal bool NoSteering;
                        [ProtoMember(22)] internal double FutureIntersectionRange;
                        [ProtoMember(23)] internal double MinTurnSpeed;
                        [ProtoMember(24)] internal bool NoTargetApproach;
                        [ProtoMember(25)] internal bool AltNavigation;
                    }

                    [ProtoContract]
                    public struct ApproachDef
                    {
                        public enum ReInitCondition
                        {
                            Wait,
                            MoveToPrevious,
                            MoveToNext,
                            ForceRestart,
                        }

                        public enum Conditions
                        {
                            Ignore,
                            Spawn,
                            DistanceFromPositionC,
                            Lifetime,
                            DesiredElevation,
                            MinTravelRequired,
                            MaxTravelRequired,
                            Deadtime,
                            DistanceToPositionC,
                            NextTimedSpawn,
                            RelativeLifetime,
                            RelativeDeadtime,
                            SinceTimedSpawn,
                            RelativeSpawns,
                            EnemyTargetLoss,
                            RelativeHealthLost,
                            HealthRemaining,
                            DistanceFromPositionB,
                            DistanceToPositionB,
                            DistanceFromTarget,
                            DistanceToTarget,
                            DistanceFromEndTrajectory,
                            DistanceToEndTrajectory,
                        }

                        public enum UpRelativeTo
                        {
                            UpRelativeToBlock,
                            UpRelativeToGravity,
                            UpTargetDirection,
                            UpTargetVelocity,
                            UpStoredStartDontUse,
                            UpStoredEndDontUse,
                            UpStoredStartPosition,
                            UpStoredEndPosition,
                            UpStoredStartLocalPosition,
                            UpStoredEndLocalPosition,
                            UpRelativeToShooter,
                            UpOriginDirection,
                            UpElevationDirection,
                        }

                        public enum FwdRelativeTo
                        {
                            ForwardElevationDirection,
                            ForwardRelativeToBlock,
                            ForwardRelativeToGravity,
                            ForwardTargetDirection,
                            ForwardTargetVelocity,
                            ForwardStoredStartDontUse,
                            ForwardStoredEndDontUse,
                            ForwardStoredStartPosition,
                            ForwardStoredEndPosition,
                            ForwardStoredStartLocalPosition,
                            ForwardStoredEndLocalPosition,
                            ForwardRelativeToShooter,
                            ForwardOriginDirection,
                        }

                        public enum RelativeTo
                        {
                            Origin,
                            Shooter,
                            Target,
                            Surface,
                            MidPoint,
                            PositionA,
                            Nothing,
                            StoredStartDontUse,
                            StoredEndDontUse,
                            StoredStartPosition,
                            StoredEndPosition,
                            StoredStartLocalPosition,
                            StoredEndLocalPosition,
                        }

                        public enum ConditionOperators
                        {
                            StartEnd_And,
                            StartEnd_Or,
                            StartAnd_EndOr,
                            StartOr_EndAnd,
                        }

                        public enum StageEvents
                        {
                            DoNothing,
                            EndProjectile,
                            EndProjectileOnRestart,
                            StoreDontUse,
                            StorePositionDontUse,
                            Refund,
                            StorePositionA,
                            StorePositionB,
                            StorePositionC,
                        }

                        [ProtoContract]
                        public struct WeightedIdListDef
                        {

                            [ProtoMember(1)] public int ApproachId;
                            [ProtoMember(2)] public Randomize Weight;
                            [ProtoMember(3)] public double End1WeightMod;
                            [ProtoMember(4)] public double End2WeightMod;
                            [ProtoMember(5)] public int MaxRuns;
                            [ProtoMember(6)] public double End3WeightMod;
                        }

                        [ProtoMember(1)] internal ReInitCondition RestartCondition;
                        [ProtoMember(2)] internal Conditions StartCondition1;
                        [ProtoMember(3)] internal Conditions EndCondition1;
                        [ProtoMember(4)] internal UpRelativeTo Up;
                        [ProtoMember(5)] internal RelativeTo PositionB;
                        [ProtoMember(6)] internal double AngleOffset;
                        [ProtoMember(7)] internal double Start1Value;
                        [ProtoMember(8)] internal double End1Value;
                        [ProtoMember(9)] internal double LeadDistance;
                        [ProtoMember(10)] internal double DesiredElevation;
                        [ProtoMember(11)] internal double AccelMulti;
                        [ProtoMember(12)] internal double SpeedCapMulti;
                        [ProtoMember(13)] internal bool AdjustPositionC;
                        [ProtoMember(14)] internal bool CanExpireOnceStarted;
                        [ProtoMember(15)] internal ParticleDef AlternateParticle;
                        [ProtoMember(16)] internal string AlternateSound;
                        [ProtoMember(17)] internal string AlternateModel;
                        [ProtoMember(18)] internal int OnRestartRevertTo;
                        [ProtoMember(19)] internal ParticleDef StartParticle;
                        [ProtoMember(20)] internal bool AdjustPositionB;
                        [ProtoMember(21)] internal bool AdjustUp;
                        [ProtoMember(22)] internal bool PushLeadByTravelDistance;
                        [ProtoMember(23)] internal double TrackingDistance;
                        [ProtoMember(24)] internal Conditions StartCondition2;
                        [ProtoMember(25)] internal double Start2Value;
                        [ProtoMember(26)] internal Conditions EndCondition2;
                        [ProtoMember(27)] internal double End2Value;
                        [ProtoMember(28)] internal RelativeTo Elevation;
                        [ProtoMember(29)] internal double ElevationTolerance;
                        [ProtoMember(30)] internal ConditionOperators Operators;
                        [ProtoMember(31)] internal StageEvents StartEvent;
                        [ProtoMember(32)] internal StageEvents EndEvent;
                        [ProtoMember(33)] internal double TotalAccelMulti;
                        [ProtoMember(34)] internal double DeAccelMulti;
                        [ProtoMember(35)] internal bool Orbit;
                        [ProtoMember(36)] internal double OrbitRadius;
                        [ProtoMember(37)] internal int OffsetTime;
                        [ProtoMember(38)] internal double OffsetMinRadius;
                        [ProtoMember(39)] internal bool NoTimedSpawns;
                        [ProtoMember(40)] internal double OffsetMaxRadius;
                        [ProtoMember(41)] internal bool ForceRestart;
                        [ProtoMember(42)] internal RelativeTo PositionC;
                        [ProtoMember(43)] internal bool DisableAvoidance;
                        [ProtoMember(44)] internal int StoredStartId;
                        [ProtoMember(45)] internal int StoredEndId;
                        [ProtoMember(46)] internal WeightedIdListDef[] RestartList;
                        [ProtoMember(47)] internal RelativeTo StoredStartType;
                        [ProtoMember(48)] internal RelativeTo StoredEndType;
                        [ProtoMember(49)] internal bool LeadRotateElevatePositionB;
                        [ProtoMember(50)] internal bool LeadRotateElevatePositionC;
                        [ProtoMember(51)] internal bool NoElevationLead;
                        [ProtoMember(52)] internal bool IgnoreAntiSmart;
                        [ProtoMember(53)] internal double HeatRefund;
                        [ProtoMember(54)] internal Randomize AngleVariance;
                        [ProtoMember(55)] internal bool ReloadRefund;
                        [ProtoMember(56)] internal int ModelRotateTime;
                        [ProtoMember(57)] internal FwdRelativeTo Forward;
                        [ProtoMember(58)] internal bool AdjustForward;
                        [ProtoMember(59)] internal bool ToggleIngoreVoxels;
                        [ProtoMember(60)] internal bool SelfAvoidance;
                        [ProtoMember(61)] internal bool TargetAvoidance;
                        [ProtoMember(62)] internal bool SelfPhasing;
                        [ProtoMember(63)] internal bool TrajectoryRelativeToB;
                        [ProtoMember(64)] internal Conditions EndCondition3;
                        [ProtoMember(65)] internal double End3Value;
                        [ProtoMember(66)] internal bool SwapNavigationType;
                        [ProtoMember(67)] internal bool ElevationRelativeToC;
                    }

                    [ProtoContract]
                    public struct MinesDef
                    {
                        [ProtoMember(1)] internal double DetectRadius;
                        [ProtoMember(2)] internal double DeCloakRadius;
                        [ProtoMember(3)] internal int FieldTime;
                        [ProtoMember(4)] internal bool Cloak;
                        [ProtoMember(5)] internal bool Persist;
                    }
                }

                [ProtoContract]
                public struct Randomize
                {
                    [ProtoMember(1)] internal float Start;
                    [ProtoMember(2)] internal float End;
                }
            }

            [ProtoContract]
            public struct ParticleOptionDef
            {
                [ProtoMember(1)] internal float Scale;
                [ProtoMember(2)] internal float MaxDistance;
                [ProtoMember(3)] internal float MaxDuration;
                [ProtoMember(4)] internal bool Loop;
                [ProtoMember(5)] internal bool Restart;
                [ProtoMember(6)] internal float HitPlayChance;
            }


            [ProtoContract]
            public struct ParticleDef
            {
                [ProtoMember(1)] internal string Name;
                [ProtoMember(2)] internal Vector4 Color;
                [ProtoMember(3)] internal Vector3D Offset;
                [ProtoMember(4)] internal ParticleOptionDef Extras;
                [ProtoMember(5)] internal bool ApplyToShield;
                [ProtoMember(6)] internal bool DisableCameraCulling;
            }
        }
    }
}