using UnityEngine;
using Skills;
using Pool;

namespace Projectiles
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Projectile : MonoBehaviour, IPoolable<Projectile>
    {
        #region Dependencies
        private SO_Projectile _data;
        #endregion
        #region Composition
        private MovementHandler _movement;
        private ObjectView _view;
        #endregion
        #region Delegates
        private event System.Action onComplete;
        #endregion
        #region Commands
        public Projectile Execute()
        {
            if (_data == null)
                return null;
        
            if(_data.movementData.movementType != MovementType.None)
                SetMovement();                

            SetView();        

            // behaviour destruction
            switch (_data.destructionCondition)
            {       
                case DestructionCondition.OnTime:
                    var timer = PoolManager.GetObject<Timer>()
                        .SetTime(_data.lifeTime)
                        .SetCompleteCallback(
                        () =>
                        {
                            _view.Stop();
                            _movement.Stop();
                            Dispose();                         
                        }
                        );
                    break;
                case DestructionCondition.OnAnimationEnd:
                    _view.SetCompleteCallback(
                        _movement.Stop,
                        Dispose
                        );
                    break;
            }

            return this;
        }
        #endregion
        #region Setup
        public Projectile SetData(SO_Projectile data)
        {
            _data = data;
            return this;
        }
        public Projectile SetPosition(Vector3 position)
        {
            transform.position = position;
            return this;
        }
        public Projectile SetRotation(Quaternion rotation) 
        {
            transform.rotation = rotation;
            return this;
        }
        public Projectile SetTarget(Vector3 position)
        {
            if(_data.movementData.movementType != MovementType.None)
                _movement.SetTarget(position);

            return this;
        }
        public Projectile SetCompleteCallback(ref System.Action callback)
        { 
            onComplete = callback;
            return this;        
        }        
        private void SetView()
        {
            _view.SetData(_data.viewData);

            if (_data.viewData.isAnimated)
                _view.Play(
                    _data.viewData.animationSpeed,
                    _data.destructionCondition != DestructionCondition.OnAnimationEnd
                    );
        }
        private void SetMovement() 
        {
            _movement
                .SetData(_data.movementData)
                .Execute(
                _data.movementData.movementType,
                () =>
                {
                    _view.Stop();
                    CheckImpactSpawn();
                    Dispose();
                }
                );
        }
        #endregion
        #region Helpers / Utils
        private void CheckImpactSpawn()
        {
            if(_data == null ) return;
            if(_data.impact == null) return;
        
            var projectile = Factory.CreateProjectile(_data.impact)
                .SetPosition(transform.position)
                .SetCompleteCallback(ref onComplete);

            onComplete = null;     
            projectile.Execute();        
        }

        #endregion
        #region Lifecycle
        public Projectile Initialize()
        {        
            if(_movement == null)        
                _movement = new MovementHandler(transform);           

            if(_view == null)
                _view = new ObjectView(GetComponent<SpriteRenderer>());

            return this;
        }
        public void Dispose()
        {
            onComplete?.Invoke();
            onComplete = null;       
            _data = null;
            transform.rotation = Quaternion.Euler(0, 0, 0);      
            PoolManager.ReturnObject(this);
        }
        #endregion
    }
}
