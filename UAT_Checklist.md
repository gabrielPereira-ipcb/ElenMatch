```markdown
# ElemenMatch - User Acceptance Testing (UAT) Checklist

This checklist is designed for end-users or game designers to test the core gameplay mechanics of ElemenMatch, particularly focusing on gravity and chemical bond detection.

**Element Key (verify in-game if different):**
*   H = Hydrogen
*   O = Oxygen
*   Na = Sodium
*   Cl = Chlorine
*   N = Nitrogen
*   C = Carbon

---

## 1. Gravity Mechanic ("Elements Falling")

| Item ID | Feature / Behavior                     | How to Test                                                                                                                               | What to Look For (Confirmation)                                                                                                | Pass/Fail | Notes |
|---------|----------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------|-----------|-------|
| 1.1     | Pieces fall into single empty spaces | Remove a piece that has another single piece directly above it.                                                                           | The piece above falls down one spot to fill the empty space.                                                                   |           |       |
| 1.2     | Stacked pieces fall correctly        | Remove a piece that is supporting a stack of 2 or more pieces.                                                                          | All pieces in the stack above the removed piece fall down, maintaining their relative order.                                   |           |       |
| 1.3     | Pieces fall to lowest point          | Create a column with a piece at the top, then one or more empty spaces, then a piece at the bottom (or empty down to the grid floor). Remove the piece(s) creating the gap(s) so the top piece has multiple empty cells to fall through. | The top piece falls all the way down to the lowest available resting spot in that column.                                      |           |       |
| 1.4     | Pieces don't fall if no space      | Have a piece resting on another piece or the floor of the grid. Try to make it fall without creating space below it.                      | The piece remains in its position and does not fall.                                                                           |           |       |

---

## 2. Link Detection System ("Chemical Bonds")

| Item ID | Feature / Behavior         | How to Test                                                                                                | What to Look For (Confirmation)                                                                                                   | Pass/Fail | Notes |
|---------|----------------------------|------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------|-----------|-------|
| 2.1     | Forming H₂O (Water)        | Gather 2 Hydrogen (H) and 1 Oxygen (O) pieces so they are all touching each other.                         | The H and O pieces combine, disappear from the grid. Score is awarded. A visual effect (e.g., animation, particle) indicates the bond. |           |       |
| 2.2     | Forming NaCl (Salt)        | Gather 1 Sodium (Na) and 1 Chlorine (Cl) pieces so they are touching each other.                           | The Na and Cl pieces combine, disappear. Score is awarded. Visual feedback occurs.                                                |           |       |
| 2.3     | Forming CO₂ (Carbon Dioxide) | Gather 1 Carbon (C) and 2 Oxygen (O) pieces so they are all touching each other.                           | The C and O pieces combine, disappear. Score is awarded. Visual feedback occurs.                                                  |           |       |
| 2.4     | Forming NH₃ (Ammonia)      | Gather 1 Nitrogen (N) and 3 Hydrogen (H) pieces so they are all touching each other.                       | The N and H pieces combine, disappear. Score is awarded. Visual feedback occurs.                                                  |           |       |
| 2.5     | Connectivity Requirement   | Place the correct number and type of elements for a known bond (e.g., H₂O) on the grid, but ensure at least one of the pieces is not touching the others needed to complete the bond structure. | The bond does *not* form. Only when all required pieces are moved to be connected (touching) does the bond occur.                |           |       |
| 2.6     | Incorrect Element Ratio    | Try to form a bond with the wrong number of elements (e.g., for H₂O, try with 1 H and 1 O, or 3 H and 1 O). | The bond does *not* form. Pieces remain on the grid.                                                                              |           |       |
| 2.7     | Efficiency/Feel            | Play the game continuously for 5-10 minutes, focusing on creating many different bonds.                    | Bond detection feels instant or very quick. There are no noticeable pauses or slowdowns when pieces fall or when bonds should form. The game feels responsive. |           |       |

---

## 3. Interaction between Gravity and Bonds

| Item ID | Feature / Behavior                     | How to Test                                                                                                                                  | What to Look For (Confirmation)                                                                                                  | Pass/Fail | Notes |
|---------|----------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------|-----------|-------|
| 3.1     | Bonds trigger falling                  | Create a setup where forming a chemical bond removes one or more pieces that are directly supporting other pieces above them.                | When the bond forms and pieces are removed, the pieces that were supported by them immediately fall down.                        |           |       |
| 3.2     | Falling triggers new bonds ("Cascades") | Arrange pieces so that when a group falls (due to a bond clearing space below), they land next to other pieces, completing a new chemical bond. | After the initial pieces fall, a new bond forms automatically with the pieces they landed near, without further player action. |           |       |

---

## 4. Game Over Conditions

| Item ID | Feature / Behavior | How to Test                                                                                                   | What to Look For (Confirmation)                                                                                                | Pass/Fail | Notes |
|---------|--------------------|---------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------|-----------|-------|
| 4.1     | Grid Full          | Continuously drop pieces until the entire playable grid area is filled with pieces. Attempt to drop one more piece. | The game recognizes the grid is full and transitions to a "Game Over" screen or state. No more pieces can be dropped.          |           |       |
| 4.2     | Column Full        | Fill one or more columns of the grid completely from bottom to top. Attempt to drop a new piece into a full column. | The piece cannot be dropped into the full column. The player retains control of the piece and can move it to other, non-full columns. |           |       |

---
```
