## Dependency injection for LeoEcsLite C# Entity Component System framework
Old, attribute-based dependency injection for [LeoECS Lite](https://github.com/Leopotam/ecslite), originally written by [Leopotam](https://github.com/Leopotam)  
*(Except I removed **EcsFilter** injection because the true way is to do it manually in systems' **Init**)*

### Table of content
* [Integration to startup](#integration-to-startup)
* [Injectors](#injectors)
    * [EcsWorld](#ecsworld)
    * [EcsPool](#ecspool)
    * [EcsShared](#ecsshared)
    * [EcsInject](#ecsinject)
* [License](#license)

### Integration to startup
```csharp
var systems = new EcsSystems (new EcsWorld ());
systems
    .Add (new System1 ())
    .AddWorld (new EcsWorld (), "events")
    // ...
    // Inject() method should be placed after
    // all systems/worlds registration and
    // before Init().
    .Inject ()
    .Init ();
```

### Injectors

#### EcsWorld
```csharp
public class TestSystem : IEcsRunSystem {
    // field will be injected with default world instance.
    [EcsWorld] EcsWorld _defaultWorld;
    
    // field will be injected with "events" world instance.
    [EcsWorld("events")] EcsWorld _eventsWorld;

    public void Run (EcsSystems systems) {
        // all injected fields can be used here.
        // _defaultWorld.xxx
        // _eventsWorld.xxx
    }
}
```

#### EcsPool
```csharp
public class TestSystem : IEcsRunSystem {
    // field will be injected with pool from default world instance.
    [EcsPool] EcsPool<C1> _c1Pool;
    
    // field will be injected with pool from "events" world instance.
    [EcsPool("events")] EcsPool<C1> _c1EventsPool;

    public void Run (EcsSystems systems) {
        // all injected fields can be used here.
        // _c1Pool.xxx
        // _c1EventsPool.xxx
    }
}
```

#### EcsShared
```csharp
public class TestSystem : IEcsRunSystem {
    // field will be injected with GetShared() instance from EcsSystems.
    [EcsShared] Shared _shared;

    public void Run (EcsSystems systems) {
        // all injected fields can be used here.
        // _shared.xxx
    }
}
```

#### EcsInject
```csharp
systems
    .Add (new TestSystem ())
    .Inject (new CustomData1 (), new CustomData2 ())
    .Init ();
// ...
public class TestSystem : IEcsRunSystem {
    // field will be injected with instance from EcsSystems.Inject() call.
    [EcsInject] CustomData1 _custom1;
    
    // field will be injected with instance from EcsSystems.Inject() call.
    [EcsInject] CustomData2 _custom2;

    public void Run (EcsSystems systems) {
        // all injected fields can be used here.
        // _custom1.xxx
        // _custom2.xxx
    }
}
```

### License
The software is released under the terms of the [MIT license](./LICENSE.md).

No personal support or any guarantees.
