# AI handoff

## Goal
Строим свой node-based PCG tool внутри Unity в духе Unreal Engine PCG Graph:
- граф редактируется в отдельном editor window;
- генерация запускается с конкретного объекта сцены через `PCGGenerator`;
- точки проходят через chain из source/filter/transform/utility/spawn нод;
- система должна быть удобной для расширения новыми нодами и не быть завязанной на хрупкие asset-path соглашения.

## Current state
Уже сделано:
- графовые ноды хранятся как `sub-assets` внутри самого `PCGGraphData`, отдельные `Data`/`*_Nodes` папки больше не используются;
- `Generate` убран из окна графа, генерация запускается из инспектора `PCGGenerator`;
- `GenerationBounds` работает как локальный box генератора, как у коллайдера, и редактируется ручками в Scene View;
- `Source` больше не использует `Grid Width/Height`, grid строится по `GenerationBounds` с учётом `Spacing`;
- executor переписан под DAG-исполнение с поддержкой нескольких входов, чтобы нормально работали `Merge` и ветвления;
- debug points больше не рисуются временными `Debug.DrawRay`, а хранятся как последний набор точек и рисуются persistent gizmos;
- ручная кнопка `Save Graph` убрана, вместо неё сделан debounced autosave;
- enum-поля нод переведены с `PopupField` на `EnumField`, чтобы dropdown не улетал в угол экрана;
- добавлены новые базовые ноды:
  - `Merge`
  - `Transform`
  - `Distance Filter`
  - `Density Noise`
  - `Attribute Set`
  - `Jitter`
- `Spawner` теперь умеет спавнить из нескольких префабов с весами;
- добавлена система `NodeRegistry + SearchWindow` для поиска и категорий при создании нод.

## Important files
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Data/PCGGraphData.cs` — основной graph asset, содержит список нод и рёбер.
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Runtime/PCGGraphExecutor.cs` — новое DAG-исполнение графа с поддержкой multi-input.
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Runtime/PCGGenerator.cs` — runtime-компонент сцены, хранит `generationBounds`, запускает граф и рисует persistent debug points.
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Editor/PCGGeneratorEditor.cs` — custom inspector и scene handle для bounds, тут же кнопка `Generate`.
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Core/PCGExecutionContext.cs` — execution context, теперь содержит `generatorTransform`.
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Data/PCGSourceNodeData.cs` — source нода, grid/random генерация внутри `GenerationBounds`.
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Data/PCGSpawnerNodeData.cs` — спавн по weighted prefab list.
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Editor/PCGSpawnerNodeView.cs` — кастомный UI списка префабов и весов для spawner.
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Editor/PCGGraphView.cs` — graph view, autosave, создание/удаление нод, связи, открытие search window.
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Editor/PCGEditorWindow.cs` — окно редактора графа, создание `PCGGraphView`, lifecycle и flush autosave.
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Core/PCGNodeParameter.cs` — описание параметров нод, сюда добавлена поддержка enum-параметров.
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Editor/PCGNodeView.cs` — generic UI параметров нод, теперь умеет `EnumField`.
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Editor/PCGNodeRegistry.cs` — реестр доступных нод для поиска и категорий.
- `C:/Users/vanya/_proj/Diplomus/Assets/Scripts/PCG/Editor/PCGNodeSearchWindow.cs` — search window для добавления нод.

## Decisions made
- Решили хранить ноды как `sub-assets` внутри `PCGGraphData`, потому что привязка к имени графа и отдельным папкам была хрупкой и неудобной.
- Не оставляли миграцию со старой asset-схемы, потому что старые графы пользователь удалил, и лишний migration-код только засоряет проект.
- Решили убрать `Generate` из `PCG Graph Editor`, потому что `FindObjectOfType<PCGGenerator>()` путал несколько объектов сцены и запускал не тот генератор.
- Решили, что `Spawner` должен только спавнить по данным точек, а рандом rotation/scale должен жить в отдельных graph-нодаx.
- Решили убрать ручной `Save Graph` и сделать autosave, потому что для editor-инструмента это удобнее.
- Не сохраняем на каждый кадр drag/slider-change, потому что это вызывало микрофризы; вместо этого используется debounce autosave.
- Решили использовать `EnumField`, а не `PopupField`, потому что у dropdown были проблемы с фокусом и позиционированием в `GraphView`.
- Решили строить добавление нод через `NodeRegistry + SearchWindow`, потому что простой список в контекстном меню плохо масштабируется.

## Known issues
- Недавние изменения вокруг `SearchWindow` и позиционирования нод после `Space`/`ПКМ` правились по коду, но не были полноценно проверены в живом Unity editor после последнего фикса с `ChangeCoordinatesTo(...)`.
- Визуально и функционально нужно руками проверить:
  - открытие search window после `pan/zoom`;
  - корректную позицию новой ноды после выбора из поиска;
  - weighted spawning в `Spawner`;
  - сохранение после drag нод и изменения полей;
  - persistent debug points после нескольких генераций.
- Автоматические editor/PlayMode тесты под эти сценарии пока не написаны.
- Нет защиты от всех возможных UX-шероховатостей `GraphView`/`UI Toolkit`; эта зона ещё живая и требует ручной обкатки.

## Next steps
1. Проверить вручную в Unity весь flow `SearchWindow -> create node -> autosave -> reopen graph`.
2. Если позиционирование нод после `Space`/`ПКМ` всё ещё плавает, добить координатные преобразования в `PCGGraphView`.
3. Продолжить расширять библиотеку нод следующими кандидатами: `Project To Surface`, `Random Rotation/Scale`, `Bounds Filter`, `Tag Filter`.
4. Подумать о более формальной системе атрибутов точек, если нод станет много.
5. При необходимости добавить editor/debug ноду для удобного просмотра состава point stream.

## Commands
- Unity compile/check запускался в основном через сам редактор Unity, а не отдельной CLI-командой.
- По месту полезно искать связанные классы так:
  - `rg "class PCG" Assets/Scripts/PCG`
  - `rg "SearchWindow|NodeRegistry|Spawner" Assets/Scripts/PCG`
- Быстрый поиск файлов:
  - `rg --files Assets/Scripts/PCG`

## Gotchas
- `PCGGraphEditor` теперь только редактирует graph asset и не должен сам выбирать объекты сцены для генерации.
- Генерация должна запускаться только через конкретный `PCGGenerator` на выбранном объекте.
- `GenerationBounds` задаются в локальном пространстве генератора, а source-конвертация переводит точки в world-space через `generatorTransform`.
- `Spawner` теперь использует уже готовые `point.position`, `point.rotation`, `point.scale`; не надо возвращать в него старую логику случайных трансформов.
- Autosave отложенный, а не мгновенный; если дебажить сохранение, важно помнить про debounce.
- Проект Unity, поэтому многие проблемы проявляются только при живой проверке в editor, даже если C# код выглядит корректно.
