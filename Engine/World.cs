using System.Collections;
using System.Collections.Generic;

using static Engine.Core;

namespace Engine
{
    public interface ICamera
    {
        Matrix View { get; }
    }

    public interface IBody
    {
        Matrix World { get; }
    }

    public interface IRender : IBody
    {
        void Draw();
        void Shape();
    }

    public abstract class World : Scene, IEnumerable<IRender>
    {
        private static World instance;

        public World()
        {
            entities = new List<Entity>();
            construct = new List<Entity>();
            destruct = new List<Entity>();
        }

        private readonly List<Entity> entities;
        private readonly List<Entity> destruct;
        private readonly List<Entity> construct;

        public void Add(Entity entity)
        {
            construct.Add(entity);
        }
        public void Remove(Entity entity)
        {
            destruct.Add(entity);
        }

        public override void Initialize()
        {
            foreach (var entity in construct)
                entities.Add(entity);

            construct.Clear();

            instance = this;
        }
        public override void Update(ITime time)
        {
            foreach (var entity in destruct)
                entities.Remove(entity);

            foreach (var entity in entities)
                entity.Update(time);

            foreach (var entity in construct)
                entities.Add(entity);

            construct.Clear();
            destruct.Clear();
        }
        public override void Draw()
        {
            
        }

        IEnumerator<IRender> IEnumerable<IRender>.GetEnumerator()
        {
            return entities.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return entities.GetEnumerator();
        }

        protected override void Dispose(bool disposing)
        {
            entities.Clear();
            destruct.Clear();
            construct.Clear();
        }

        public abstract class Entity : IRender
        {
            protected static void Add(Entity entity)
            {
                instance.Add(entity);
            }
            protected static void Remove(Entity entity)
            {
                instance.Remove(entity);
            }

            public virtual Matrix World { get; set; } = Matrix.One;

            public abstract void Update(ITime time);
            public abstract void Draw();
            public abstract void Shape();
        }
    }

    public abstract class DynamicBody : World.Entity
    {
        protected Vector VelocityL { get; set; }
        protected Vector VelocityA { get; set; }

        public override void Update(ITime time)
        {
            World = Matrix.CreateTranslation(time.Elapsed * VelocityL) * World * Matrix.CreateRotation(time.Elapsed * VelocityA);
        }
    }
}