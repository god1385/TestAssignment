# TestAssignment
2d Project with Drag&amp;Drop
Unity Version - 2022.3.62f3 LTS

🧱 Tower Cubes

A small Unity project that implements a drag & drop tower-building mechanic with cube removal and animated tower rebuild.

The project focuses on clean architecture, separation of concerns, and test-task–friendly structure.

🧠 Architecture Overview

The project is split into clear layers:

TowerService — core game logic (placement, removal, rebuild, save/load)

TowerState — pure data state of the tower

TowerView — visuals, animations, user input handling

CubeView / DraggableCube — UI representation and drag logic

👉 No View objects are stored inside the Service or State layers.

🧱 TowerState & Data Model

TowerState is the single source of truth for the tower:

Stores only data (TowerCube)

Has no Unity dependencies

Is fully controlled by TowerService

TowerCube is a lightweight struct representing the cube state (position, size, sprite id).

🎯 Placement & Rules

Cube placement is handled via interfaces:

ITowerPlacement — calculates drop position and placement result

ICubePlacement — validates custom placement rules (e.g. color matching)

This allows easy extension without modifying existing logic.

🔄 Removal & Rebuild Flow

User drags a cube into the hole zone

TowerService validates removal

Cube is removed from TowerState

Service calculates new target positions

TowerView animates rebuild and applies updated positions

Rebuild logic is calculated in the Service, animation is handled in the View.

💾 Save / Load

Tower state is serialized via ISaveService

Only pure data is saved

Visual state is fully reconstructed on load

🛠 Technologies

Unity UI (RectTransform)

DOTween

Zenject (DI)

JSON save system

🎥 Demo

## Placing cubes

![Placing cube](Gifs/DefaultMechanic.gif)

## Removing cubes

![Removing cube](Gifs/HoleThrow.gif)
