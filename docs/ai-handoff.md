# AI handoff

## Goal
Строим свой node-based PCG editor внутри Unity, по духу ближе к Unreal PCG Graph: граф как asset, генерация через `PCGGenerator` на объектах сцены, базовый набор нод для point-processing, удобный editor workflow и понятная debug-визуализация.

## Current state
Сейчас уже сделано:

- `PCGGraphData` хранит граф и node sub-assets внутри одного graph asset.
- `PCG Graph Editor` больше не запускает генерацию в сцене, а только редактирует graph asset.
- Генерация запускается с конкретного `PCGGenerator` через custom inspector.
- `GenerationBounds` у `PCGGenerator` редактируется в сцене как box handle.
- `Source Node` больше не использует `Grid Width/Height`, а раскладывает grid по `generationBounds` с шагом `Spacing`.
- `PCGGraphExecutor` переписан под DAG-исполнение с накоплением входов, чтобы работал `Merge` и multi-input pipeline.
- `Spawner Node` теперь использует список префабов с весами и выбирает prefab по weighted random.
- Добавлены базовые ноды: `Merge`, `Transform`, `Distance Filter`, `Density Noise`, `Attribute Set`, `Jitter`.
- Сохранение графа переведено на autosave с debounce, чтобы не было микрофризов при drag/edit.
- Enum-поля вроде `Source Type` и `Filter Type` переведены с `PopupField` на `EnumField`, чтобы dropdown не улетал в угол окна.
- `debug points` переведены с временного `Debug.DrawRay` на persistent gizmo visualization через последний набор `lastGeneratedPoints`.

## Important files
- `Assets/Scripts/PCG/Runtime/PCGGraphExecutor.cs` — ключевая логика исполнения графа, сейчас работает как DAG с накоплением входов.
- `Assets/Scripts/PCG/Runtime/PCGGenerator.cs` — runtime entry point на объекте сцены; здесь generation bounds, запуск графа и debug points.
- `Assets/Scripts/PCG/Editor/PCGGeneratorEditor.cs` — inspector кнопка `Generate` и scene handle для bounds.
- `Assets/Scripts/PCG/Editor/PCGEditorWindow.cs` — окно graph editor, теперь без ручного save и без scene generation.
- `Assets/Scripts/PCG/Editor/PCGGraphView.cs` — создание/удаление нод, связей и autosave c debounce.
- `Assets/Scripts/PCG/Editor/PCGNodeView.cs` — общий UI для параметров нод, включая `EnumField`.
- `Assets/Scripts/PCG/Core/PCGNodeParameter.cs` — описание параметров нод, добавлен `Enum`-тип и `CreateEnum(...)`.
- `Assets/Scripts/PCG/Data/PCGSourceNodeData.cs` — source logic на основе `generationBounds`.
- `Assets/Scripts/PCG/Data/PCGSpawnerNodeData.cs` — weighted prefab spawning.
- `Assets/Scripts/PCG/Editor/PCGSpawnerNodeView.cs` — кастомный UI списка `prefab + weight` внутри spawner-ноды.

## Decisions made
- Решили хранить ноды как sub-assets внутри graph asset, потому что привязка к отдельным папкам `Data`/`*_Nodes` оказалась хрупкой и неудобной.
- Не используем ручной `Generate` в `PCG Graph Editor`, потому что `FindObjectOfType<PCGGenerator>()` путал несколько генераторов в сцене.
- Не используем ручную кнопку `Save Graph`, потому что граф переведён на autosave.
- Не сохраняем graph на каждый кадр drag/edit, потому что это вызывало микрофризы; используем debounce autosave.
- Не оставляем `Random Rotation` в `Spawner`, потому что ответственность за трансформацию точек должна жить в graph nodes, а не в spawn step.
- Не используем `PopupField` для enum-полей `SourceType`/`FilterType`, потому что после возврата фокуса в окно графа popup иногда открывался в углу экрана.
- Debug points рисуем как persistent gizmos, а не через корутину с `Debug.DrawRay`, потому что старые наборы наслаивались и потом исчезали через 5 секунд.

## Known issues
- Unity-компиляция и живые editor-проверки после части последних изменений не прогонялись из Codex-среды; нужно проверять руками в Unity.
- В `PCGNodeParameter` всё ещё есть legacy `Dropdown`-ветка, хотя `Source Type` и `Filter Type` уже переведены на `EnumField`.
- `PCGGraphExecutor` пока не вводит явную защиту от циклов на уровне editor graph validation; при цикле он логирует проблему через количество обработанных нод, но лучше не строить циклические графы.
- Нет отдельного `Project To Surface` / `Random Rotation/Scale` node, хотя это были следующие приоритетные кандидаты.
- Для `Spawner Node` нет красивых заголовков `Prefab` / `Weight` в каждой строке, UI минималистичный.

## Next steps
1. Открыть Unity и проверить компиляцию после последних изменений, особенно `Spawner Node` с weighted prefabs и `EnumField` для enum-параметров.
2. Протестировать несколько графов: `Merge`, `Density Noise + Filter`, `Spawner` с несколькими префабами и весами.
3. Добавить следующие приоритетные ноды вроде `Project To Surface` и/или отдельную `Random Rotation/Scale` node.
4. При желании добавить graph validation для циклов и, возможно, более явную визуальную диагностику invalid graphs.

## Commands
- Unity Editor manual compile / playmode check
- Внутри этой сессии отдельные `npm`/`dotnet`/`cargo` команды не использовались

## Gotchas
- `PCG Graph Editor` редактирует graph asset, но не должен управлять тем, какой объект сцены генерирует результат.
- Генерация должна запускаться с конкретного `PCGGenerator` в сцене.
- `generationBounds` трактуется как локальный bounds генератора, а точки переводятся в world-space через `generatorTransform`.
- Autosave графа отложенный; при закрытии окна/view делается flush.
- `debugDrawPoints` сейчас показывает только последний результат генерации, не историю запусков.
- `Spawner Node` игнорирует строки без prefab или с весом `<= 0`.
