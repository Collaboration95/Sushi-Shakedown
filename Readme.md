# Sushi-Shakedown Game Reference 

## Guru's Corner

### Features Checklist
- [x] NPC Customers moving around (similar to Pixel Cafe)
- [x] Customers requesting dishes
    - [x] Basic dish assets (default style)
    - [x] Request display system
- [x] Customer patience system
    - [x] Timer implementation
    - [x] Visual indicator (PatienceBar)
- [x] Customers leaving when patience timer expires
- [x] Customer states (entering, waiting, satisfied, angry)
- [x] Simple sound effects for key events
- [x] Basic scoring system
- [ ] Game over condition
- [ ] Customer variety (different sprites or colors)
- [ ] Tutorial/instructions screen


### 🧾 **Checkpoint 2 priority**

- [x] Convert text bubble from `...` to show food order when customer reaches their `OrderArea`.
- [x] Implement a **patience timer**:
  - [x] Customer leaves when timer runs out.
  - [x] Score is reduced when customer leaves angrily.
- [x] Add a **BoxCollider2D** to the food bubble (for interaction detection).
- [x] Implement a **data structure** for food orders (e.g., ScriptableObject or class with food type, ID, sprite).
- [x] Implement **drag-and-drop** for food items:
  - [x] Drag food from prep area.
  - [x] Drop onto customer or their order bubble.
- [x] Add **animation/sprite change for customer mood**:
  - [x] Happy when given correct food.
  - [x] Angry if time runs out or wrong food is given.
  - [x] (Can spoof with `OnClick()` trigger for testing).

---
### Documentation 
---

### NPCController.cs
- **Purpose**: Controls NPC movement and behavior.
- **Key Points**:
  - Moves NPCs at a set speed and flips sprite based on direction.
  - Destroys NPC when it goes off-screen.
  - Aligns the bottom of the sprite to a given Y-coordinate.
- **Usage**: Attach to your NPC prefab (requires a SpriteRenderer). No major changes.

---

### SpawnZoneData.cs
- **Purpose**: Defines vertical boundaries for NPC and Customer spawns.
- **Key Points**:
  - Uses a BoxCollider2D to calculate minY and maxY at runtime.
- **Usage**: Attach to a spawn zone object (requires a BoxCollider2D). No changes in logic.

---

### NPCSpawner.cs
- **Purpose**: Spawns both NPCs and Customers into the scene.
- **Key Points**:
  - Spawns NPCs at regular intervals.
  - Spawns Customers manually via the `spawnCustomerDebug` flag.
  - Spawns Customers only if a free `OrderArea` is available (managed by `OrderAreaGroup`).
  - Customer spawn and NPC spawn logic are separated into `SpawnCustomer()` and `SpawnNPC()`.
- **New Dependency**: Requires an `OrderAreaGroup` reference to manage available order spots.
- **Usage**:
  - Attach to a spawner object.
  - Assign references to `npcTemplate`, `customerTemplate`, `spawnZone`, and `orderAreaGroup`.
  - Toggle `spawnCustomerDebug` to manually spawn a customer.

---

### CustomerController.cs
- **Purpose**: Central controller for customer behavior including movement, ordering, waiting, and reactions.
- **Key Components**:
  - **Movement System**: Handles customer navigation to assigned counter spots and exit paths
  - **Order Management**: Controls order creation and verification via OrderBubble
  - **Patience System**: Implements declining patience over time with visual feedback
  - **State Management**: Manages customer states (walking, waiting, satisfied, angry)
  - **Scoring Integration**: Calculates and updates score based on service speed
  
- **Key Properties**:
  - `speed`: Movement speed of the customer
  - `maxPatience`: Maximum patience value (100 by default)
  - Various sprites to show different emotional states (happy, frustrated, angry)
  - References to PatienceBar, OrderBubble, ScoreCounter, and FoodManager

- **Key Methods**:
  - `SetOrderArea(OrderArea)`: Assigns destination counter position
  - `OnCorrectDelivery()`: Increases patience when correct food is delivered
  - `OnWrongDelivery(string)`: Processes wrong food delivery, fails order
  - `OnAllOrdersFulfilled()`: Completes the order process when all items delivered
  - `ArrivedAtCounter()`: Activates order bubble and starts patience countdown
  - `CountTo100()`: Coroutine that decreases patience over time (10 seconds)
  - `OrderCompleted()/OrderFailed()`: Handle successful/failed orders with scoring and animations
  - `SetOffScreenTarget()`: Intelligently determines exit path based on screen position

- **Behavior Flow**:
  1. Customer walks to assigned OrderArea
  2. On arrival, displays order and starts patience timer
  3. Patience decreases over time (visualized by PatienceBar)
  4. Reacts to correct/incorrect food deliveries
  5. When order completes or fails, updates score and walks off-screen
  6. Customer sprite changes based on satisfaction level

- **Integration**:
  - Called by OrderBubble when food is delivered
  - Works with OrderArea to manage counter positions
  - Updates ScoreCounter based on service speed
  - Uses PatienceBar to visualize waiting time
  - Plays audio feedback on order completion/failure

- **Usage**:
  - Attach to customer prefabs
  - Requires references to OrderBubble, PatienceBar, ScoreCounter
  - Configure sprites for different emotional states
  - Audio clips can be assigned for success/failure feedback

---

### OrderArea.cs
- **Purpose**: Represents a single spot at the counter where a Customer can order.
- **Key Points**:
  - Tracks if the spot is currently occupied.
  - Provides coordinates for customer navigation.
  - Contains a trigger event for customer presence.
- **Usage**:
  - Attach to counter spot GameObjects.
  - Each should have a Collider2D set to trigger.
  - Called by the spawner and customer to set availability.

---

### OrderAreaGroup.cs
- **Purpose**: Manages multiple `OrderArea` components as a pool.
- **Key Points**:
  - Provides a method to fetch the first free order spot.
  - Can reset/release an occupied spot later via `ReleaseOrderArea()`.
- **Usage**:
  - Attach to a GameObject containing multiple child `OrderArea` objects.
  - Automatically collects them on Awake().
  - Used by `NPCSpawner` to assign a valid order spot to a new customer.

---

### OrderBubble.cs
- **Purpose**: Manages the customer's food order display and food delivery interactions.
- **Key Components**:
  - **Order Display**: Shows food items customers want in visual bubble
  - **Order Management**: Handles adding, removing, and validating food orders
  - **Food Positioning**: Calculates placement of food icons within bubble
  - **Delivery Validation**: Processes player's food deliveries
  
- **Key Properties**:
  - `maxSlots`: Maximum number of food items per order (3 by default)
  - `slotSpacing`: Controls vertical spacing between order items
  - `deliveryAnimationDuration`: Time for delivery animation
  
- **Key Methods**:
  - `StartOrder(int)`: Creates 1-3 random food orders and positions them in bubble
  - `GetOrders()`: Returns current list of pending orders
  - `ProcessFoodDelivery(FoodDraggable)`: Validates delivered food against orders
  - `CalculateOrderPosition(int)`: Determines position of each food item in bubble
  - `AnimateDeliveryAndCleanup()`: Visual animation for successful delivery

- **Behavior Flow**:
  1. Initialized when customer arrives at counter
  2. Creates 1-3 random food orders using FoodManager
  3. Positions food icons within bubble using calculated spacing
  4. Detects food dragged into bubble via trigger collisions
  5. Validates food type against pending orders
  6. Animates successful deliveries and removes from order list
  7. Notifies CustomerController of delivery results

- **Integration**:
  - Accesses FoodManager to generate random orders
  - Communicates with CustomerController about delivery outcomes
  - Detects collision with FoodDraggable items

- **Usage**:
  - Attach to a bubble GameObject in customer prefab
  - Requires BoxCollider2D set as trigger
  - Needs reference to FoodManager
  - Should be positioned relative to customer

---

### FoodDraggable.cs
- **Purpose**: Enables drag-and-drop interaction for food items.
- **Key Components**:
  - **Drag Mechanics**: Handles mouse-based food item dragging
  - **Position Management**: Tracks original position for returning
  - **Animation**: Smooth movement for returning to original position
  
- **Key Properties**:
  - Inherits from base `Food` class (food type, name, sprite)
  - Tracks original position for return animation
  
- **Key Methods**:
  - `OnMouseDown()`: Initiates dragging with proper offset calculation
  - `OnMouseDrag()`: Updates position during drag
  - `OnMouseUp()`: Returns food to original position
  - `SmoothReturn()`: Coroutine for animated return
  - `GetMouseWorldPosition()`: Utility for converting screen to world coordinates

- **Behavior Flow**:
  1. Player clicks on food item to start drag
  2. Item follows mouse cursor during drag
  3. On release, item smoothly returns to original position
  4. During drag, can collide with OrderBubble to trigger delivery

- **Integration**:
  - Extends base Food class
  - Works with OrderBubble trigger system for delivery
  - Uses Unity's mouse event system

- **Usage**:
  - Attach to food item prefabs
  - Requires Collider2D component for trigger detection
  - Position in food prep area of scene

--- 
#### Audio Assets 
Audio when customer reaches counter
Audio when customer recieves order (very good , good , default , bad)
Audo when customer times out 
