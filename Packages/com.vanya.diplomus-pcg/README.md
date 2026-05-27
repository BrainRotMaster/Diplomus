# Diplomus PCG

Diplomus PCG is a Unity editor package for node-based procedural content generation.
It provides a graph editor, runtime graph execution, point-processing nodes, region
components, and a scene generator component.

## Usage

Open the graph editor from `Tools > PCG Graph Editor`.

Create or load a `PCGGraphData` asset, build a graph from source, filter,
transform, utility, attribute, and spawn nodes, then assign the graph to a
`PCGGenerator` component in the scene. Generation is started from the generator
inspector.

## Package Layout

- `Runtime/Core`: point, edge, parameter, output, and execution context types.
- `Runtime/Data`: graph and node data assets.
- `Runtime/Components`: generator, executor, and region MonoBehaviours.
- `Editor/GraphEditor`: graph editor, node views, inspectors, and asset handlers.
