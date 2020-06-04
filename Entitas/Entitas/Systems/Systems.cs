using System.Collections.Generic;

namespace Entitas {

    /// Systems provide a convenient way to group systems.
    /// You can add IInitializeSystem, IExecuteSystem, ICleanupSystem,
    /// ITearDownSystem, ReactiveSystem and other nested Systems instances.
    /// All systems will be initialized and executed based on the order
    /// you added them.
    public class Systems : IInitializeSystem, IExecuteSystem, ICleanupSystem, ITearDownSystem {

        protected readonly List<IInitializeSystem> _initializeSystems;
        protected readonly List<IExecuteSystem> _executeSystems;
        protected readonly List<ICleanupSystem> _cleanupSystems;
        protected readonly List<ITearDownSystem> _tearDownSystems;

        public virtual SystemPriority Priority { get { return SystemPriority.Normal; } }

        /// Creates a new Systems instance.
        public Systems() {
            _initializeSystems = new List<IInitializeSystem>();
            _executeSystems = new List<IExecuteSystem>();
            _cleanupSystems = new List<ICleanupSystem>();
            _tearDownSystems = new List<ITearDownSystem>();
        }

        protected void AddHighestSystem<T>( List<T> list, T system )
            where T : class, ISystem {

            var index = list.FindLastIndex(item => { return item.Priority == SystemPriority.Highest; });

            list.Insert(index == -1 ? 0 : index + 1, system);
        }

        protected void AddHighSystem<T>( List<T> list, T system )
            where T : class, ISystem {

            var index = list.FindLastIndex(item => { return item.Priority == SystemPriority.High; });

            if (index == -1) {
                AddHighestSystem(list, system);
            } else {
                list.Insert(index + 1, system);
            }
        }

        protected void AddNormalSystem<T>( List<T> list, T system )
            where T : class, ISystem {

            var index = list.FindLastIndex(item => { return item.Priority == SystemPriority.Normal; });

            if (index == -1) {
                AddHighSystem(list, system);
            } else {
                list.Insert(index + 1, system);
            }
        }

        protected void AddLowSystem<T>( List<T> list, T system )
            where T : class, ISystem {

            var index = list.FindLastIndex(item => { return item.Priority == SystemPriority.Normal; });

            if (index == -1) {
                AddNormalSystem(list, system);
            } else {
                list.Insert(index + 1, system);
            }
        }

        protected void AddLowestSystem<T>( List<T> list, T system )
            where T : class, ISystem {

            var index = list.FindLastIndex(item => { return item.Priority == SystemPriority.Normal; });

            if (index == -1) {
                AddLowSystem(list, system);
            } else {
                list.Insert(index + 1, system);
            }
        }

        protected void SystemAdd<T>( List<T> list, T system ) 
            where T : class, ISystem {

            switch (system.Priority) {
                case SystemPriority.Highest:
                    AddHighestSystem(list, system);
                    break;
                
                case SystemPriority.High:
                    AddHighSystem(list, system);
                    break;

                case SystemPriority.Normal:
                    AddNormalSystem(list, system);
                    break;

                case SystemPriority.Low:
                    AddLowSystem(list, system);
                    break;
                
                case SystemPriority.Lowest:
                    AddLowestSystem(list, system);
                    break;

                default:
                    break;
            }
        }

        /// Adds the system instance to the systems list.
        public virtual Systems Add(ISystem system) {
            var initializeSystem = system as IInitializeSystem;
            if (initializeSystem != null) {
                SystemAdd(_initializeSystems, initializeSystem);
            }

            var executeSystem = system as IExecuteSystem;
            if (executeSystem != null) {
                SystemAdd(_executeSystems, executeSystem);
            }

            var cleanupSystem = system as ICleanupSystem;
            if (cleanupSystem != null) {
                SystemAdd(_cleanupSystems, cleanupSystem);
            }

            var tearDownSystem = system as ITearDownSystem;
            if (tearDownSystem != null) {
                SystemAdd(_tearDownSystems, tearDownSystem);
            }

            return this;
        }

        /// Calls Initialize() on all IInitializeSystem and other
        /// nested Systems instances in the order you added them.
        public virtual void Initialize() {
            for (int i = 0; i < _initializeSystems.Count; i++) {
                _initializeSystems[i].Initialize();
            }
        }

        /// Calls Execute() on all IExecuteSystem and other
        /// nested Systems instances in the order you added them.
        public virtual void Execute() {
            for (int i = 0; i < _executeSystems.Count; i++) {
                _executeSystems[i].Execute();
            }
        }

        /// Calls Cleanup() on all ICleanupSystem and other
        /// nested Systems instances in the order you added them.
        public virtual void Cleanup() {
            for (int i = 0; i < _cleanupSystems.Count; i++) {
                _cleanupSystems[i].Cleanup();
            }
        }

        /// Calls TearDown() on all ITearDownSystem  and other
        /// nested Systems instances in the order you added them.
        public virtual void TearDown() {
            for (int i = 0; i < _tearDownSystems.Count; i++) {
                _tearDownSystems[i].TearDown();
            }
        }

        /// Activates all ReactiveSystems in the systems list.
        public void ActivateReactiveSystems() {
            for (int i = 0; i < _executeSystems.Count; i++) {
                var system = _executeSystems[i];
                var reactiveSystem = system as IReactiveSystem;
                if (reactiveSystem != null) {
                    reactiveSystem.Activate();
                }

                var nestedSystems = system as Systems;
                if (nestedSystems != null) {
                    nestedSystems.ActivateReactiveSystems();
                }
            }
        }

        /// Deactivates all ReactiveSystems in the systems list.
        /// This will also clear all ReactiveSystems.
        /// This is useful when you want to soft-restart your application and
        /// want to reuse your existing system instances.
        public void DeactivateReactiveSystems() {
            for (int i = 0; i < _executeSystems.Count; i++) {
                var system = _executeSystems[i];
                var reactiveSystem = system as IReactiveSystem;
                if (reactiveSystem != null) {
                    reactiveSystem.Deactivate();
                }

                var nestedSystems = system as Systems;
                if (nestedSystems != null) {
                    nestedSystems.DeactivateReactiveSystems();
                }
            }
        }

        /// Clears all ReactiveSystems in the systems list.
        public void ClearReactiveSystems() {
            for (int i = 0; i < _executeSystems.Count; i++) {
                var system = _executeSystems[i];
                var reactiveSystem = system as IReactiveSystem;
                if (reactiveSystem != null) {
                    reactiveSystem.Clear();
                }

                var nestedSystems = system as Systems;
                if (nestedSystems != null) {
                    nestedSystems.ClearReactiveSystems();
                }
            }
        }
    }
}
