namespace Entitas {
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseSystem : ISystem {
        /// <summary>
        /// 
        /// </summary>
        public virtual SystemPriority Priority {
            get {
                return SystemPriority.Normal;
            }
        }
    }
}