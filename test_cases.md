```markdown
# Test Cases for GridManager Mechanics

This document outlines test cases for the gravity and chemical bond mechanics in `GridManager.cs`. Element types are mapped as: H=0, O=1, Na=2, Cl=3, N=4, C=5 (Note: CO2 uses C=5, O=1 as per current `chemicalBonds` if Carbon is element type 5. Please verify element type for Carbon in `GridManager.chemicalBonds` if it's different, e.g., if it's 3 as per the example `chemicalBonds` in the prompt, then it's {3, new Dictionary<int, int> { { 3, 1 }, { 1, 2 } } } meaning element 3 is Carbon. The descriptions below will assume C=5 based on `ElementPiece.cs` data, and H2O, NaCl, NH3 mapping to their respective indices 0,1,2,3,4. Adjust element types in setup based on the final `chemicalBonds` dictionary in `GridManager.cs`).

**Referenced Element Data (from ElementPiece.cs):**
*   0: H (Hydrogen)
*   1: O (Oxygen)
*   2: Na (Sodium)
*   3: Cl (Chlorine)
*   4: N (Nitrogen)
*   5: C (Carbon)

**Referenced Chemical Bonds (from GridManager.cs - verify exact recipes and element type for Carbon):**
*   H₂O: 2xH (0), 1xO (1)
*   NaCl: 1xNa (2), 1xCl (3)
*   CO₂: 1xC (5), 2xO (1) - *Assuming Carbon is element type 5*
*   NH₃: 1xN (4), 3xH (0)

---

## A. Gravity Tests (Leverage `MakePiecesFall()` logs)

**GRAV_01: Single piece falls into an empty space directly below.**
*   **Objective:** Verify a single piece falls one step into an empty cell immediately below it when the cell above it is cleared.
*   **Setup:**
    1.  Start the game.
    2.  Drop pieces to create the following vertical arrangement in a column (e.g., Column 0):
        *   `[0,0]` (Bottom): Piece A (any type)
        *   `[0,1]`: Piece B (any type, e.g., H)
        *   `[0,2]`: Piece C (any type, e.g., H)
    3.  Arrange other pieces (e.g., one O piece at `[1,1]`) such that Piece B and the O piece form a bond when another H is dropped at `[2,1]` to complete H2O. The goal is to remove Piece B.
*   **Steps:**
    1.  Drop a piece (e.g. H at `[2,1]`) that forms a bond with Piece B (e.g., H at `[0,1]`) and the O piece at `[1,1]`, causing Piece B to be removed. This should leave `[0,1]` empty.
*   **Expected Outcome:**
    *   **Visual:** Piece C (originally at `[0,2]`) falls into `[0,1]`.
    *   **Grid Data:** `grid[0,1]` should now contain Piece C. `grid[0,2]` should be null.
    *   **Logs:**
        *   "MakePiecesFall() called."
        *   "Checking column 0 for falling pieces."
        *   "Found empty row at 1 in column 0." (after Piece B is removed and before C falls)
        *   "Found piece at [0, 2] to move to [0, 1]." (referring to Piece C)
        *   "Moving piece of type: {type of C}"
        *   "Piece moved to grid[0, 1] and position updated."
        *   "Pieces fell, re-triggering bond checks for X specific locations." (X should be >= 1)

---

**GRAV_02: Multiple stacked pieces fall when a piece below them is removed.**
*   **Objective:** Verify that a stack of pieces falls correctly when a supporting piece below them is removed.
*   **Setup:**
    1.  Start the game.
    2.  Drop pieces to create:
        *   `[0,0]`: Piece X (e.g., H) - This will be removed.
        *   `[0,1]`: Piece A (any type)
        *   `[0,2]`: Piece B (any type)
        *   `[0,3]`: Piece C (any type)
    3.  Arrange other pieces (e.g., one O piece at `[1,0]`) so that Piece X forms a bond and is removed when another H is dropped at `[2,0]`.
*   **Steps:**
    1.  Drop a piece (e.g. H at `[2,0]`) that forms a bond with Piece X, causing Piece X to be removed. This leaves `[0,0]` empty.
*   **Expected Outcome:**
    *   **Visual:** Pieces A, B, and C fall down one level. A to `[0,0]`, B to `[0,1]`, C to `[0,2]`.
    *   **Grid Data:** `grid[0,0]` = A, `grid[0,1]` = B, `grid[0,2]` = C. `grid[0,3]` = null.
    *   **Logs:**
        *   "MakePiecesFall() called."
        *   "Checking column 0 for falling pieces."
        *   "Found empty row at 0 in column 0."
        *   "Found piece at [0, 1] to move to [0, 0]." (Piece A)
        *   "Piece moved to grid[0, 0] and position updated."
        *   "Found piece at [0, 2] to move to [0, 1]." (Piece B)
        *   "Piece moved to grid[0, 1] and position updated."
        *   "Found piece at [0, 3] to move to [0, 2]." (Piece C)
        *   "Piece moved to grid[0, 2] and position updated."
        *   "Pieces fell, re-triggering bond checks for X specific locations."

---

**GRAV_03: Piece does not fall if there is no empty space directly below it in the column.**
*   **Objective:** Verify that pieces do not fall if their column is full up to their position.
*   **Setup:**
    1.  Start the game.
    2.  Fill column 0 completely, from `[0,0]` to `[0,height-1]`.
    3.  In column 1, place a piece at `[1,0]` (Piece A) and another at `[1,1]` (Piece B).
*   **Steps:**
    1.  Attempt to remove a piece from column 0 (e.g., by forming a bond). This might be tricky if the column is full and no bonds can be made.
    2.  Alternatively, after setting up column 1, drop a piece in column 0. Then, remove a piece in column 1 that is *not* supporting A or B (e.g. a piece at `[1,2]`). The key is to trigger `MakePiecesFall` while A and B have no space below them.
*   **Expected Outcome:**
    *   **Visual:** Pieces A and B in column 1 should not move.
    *   **Grid Data:** `grid[1,0]` remains A, `grid[1,1]` remains B.
    *   **Logs:**
        *   "MakePiecesFall() called."
        *   "Checking column 1 for falling pieces."
        *   If the column is full from the bottom, logs like "Found empty row..." should not appear for the base of that column, or `firstEmptyRow` would be -1 (or top of column if fully empty).
        *   No logs indicating pieces A or B were found "to move".
        *   If other pieces fell elsewhere, "Pieces fell, re-triggering bond checks..." might appear, but the checks for column 1 should not lead to A or B moving.

---

**GRAV_04: Piece falls multiple steps to the lowest available spot in a column.**
*   **Objective:** Verify a piece falls through multiple empty cells to the lowest available position.
*   **Setup:**
    1.  Start the game.
    2.  In column 0:
        *   `[0,0]`: Piece A (any type) - to create a base.
        *   `[0,1]`: Empty
        *   `[0,2]`: Empty
        *   `[0,3]`: Piece B (any type) - this is the piece that will fall.
    3.  To create the empty spaces at `[0,1]` and `[0,2]`:
        *   First, place temporary pieces (Temp1 at `[0,1]`, Temp2 at `[0,2]`).
        *   Then, arrange pieces in other columns to form bonds and remove Temp1 and Temp2 sequentially, without disturbing Piece A or Piece B. This is complex.
        *   A simpler way if direct setup isn't possible: Stack `A, Temp1, Temp2, B`. Then remove `Temp1` and `Temp2` simultaneously if possible via a bond that affects both (e.g., if Temp1 and Temp2 are part of a larger bond).
*   **Steps:**
    1.  Ensure `[0,1]` and `[0,2]` are empty. Piece B is at `[0,3]`. Piece A is at `[0,0]`.
    2.  Trigger a change that calls `MakePiecesFall()`. This could be removing an unrelated piece in another column.
*   **Expected Outcome:**
    *   **Visual:** Piece B falls from `[0,3]` to `[0,1]`.
    *   **Grid Data:** `grid[0,1]` = Piece B. `grid[0,3]` = null.
    *   **Logs:**
        *   "MakePiecesFall() called."
        *   "Checking column 0 for falling pieces."
        *   "Found empty row at 2 in column 0." (or 1, depending on exact state when `MakePiecesFall` is called relative to piece removal)
        *   "Found piece at [0, 3] to move to [0, 1]." (or [0,2] then [0,1] in sequence if the logic processes one step at a time per `MakePiecesFall` call, but current code implies it finds the final `firstEmptyRow` and moves the highest piece to the current `firstEmptyRow`, then decrements `firstEmptyRow` and repeats for that column. So it should be a direct move to the final spot in one pass for that piece).
        *   "Piece moved to grid[0, 1] and position updated."
        *   "Pieces fell, re-triggering bond checks for X specific locations."

---

**GRAV_05: Confirm bond checking is re-triggered for relevant locations after a fall.**
*   **Objective:** Verify that after pieces fall, `CheckForChemicalBonds` is called for the locations where pieces have moved or landed, and their new neighbors.
*   **Setup:**
    1.  Start the game.
    2.  Create a situation where a piece (Piece A, e.g. H type) will fall.
    3.  Below the spot where Piece A will land, pre-place pieces that *would* form a bond with Piece A if it were there. For example:
        *   `[0,0]`: Piece O (Oxygen)
        *   `[1,0]`: Piece H (Hydrogen)
        *   `[0,1]`: Empty (this is where Piece A will land)
        *   `[0,2]`: Piece A (Hydrogen, which will fall to `[0,1]`)
*   **Steps:**
    1.  Trigger conditions for Piece A to fall from `[0,2]` to `[0,1]`. (e.g. by removing a piece that was at `[0,1]`)
*   **Expected Outcome:**
    *   **Visual:** Piece A falls to `[0,1]`. Then, Piece A at `[0,1]`, Piece O at `[0,0]` and Piece H at `[1,0]` should form an H₂O bond and be removed.
    *   **Logs:**
        *   Logs for Piece A falling (as in GRAV_01).
        *   "Pieces fell, re-triggering bond checks for X specific locations." (X should be > 0).
        *   The locations listed for checking should include `[0,1]` (where A landed) and its neighbors `[0,0]` and `[1,0]`.
        *   "CheckForChemicalBonds called for piece at [0, 1]." (or [0,0] or [1,0] depending on iteration order).
        *   Logs indicating an H₂O bond was found and processed (see BOND_H2O_01).

---

## B. Link Detection (Chemical Bond) Tests (Leverage `CheckForChemicalBonds()` logs)

**General Setup for Bond Tests:**
*   Carefully drop pieces to achieve the specified configurations. This will require patience.
*   Pay attention to the `elementType` values (H=0, O=1, Na=2, Cl=3, N=4, C=5).
*   "Connected" means pieces are orthogonally adjacent (share a side).

---

**BOND_H2O_01: Successful H₂O formation (2xH, 1xO, connected).**
*   **Objective:** Verify H₂O bond (2 Hydrogen, 1 Oxygen) forms and removes pieces.
*   **Setup:**
    1.  `[0,0]`: Piece H (type 0)
    2.  `[0,1]`: Piece O (type 1)
    3.  `[1,0]`: Piece H (type 0)
    (Ensure these are connected, e.g. O is adjacent to both Hs).
*   **Steps:**
    1.  The bond should form automatically when the last piece completing the pattern and connectivity is dropped. Or, if pieces are arranged and then another piece drop triggers `CheckForChemicalBonds` for one of them.
*   **Expected Outcome:**
    *   **Visual:** The three pieces (2H, 1O) are removed from the grid.
    *   **Grid Data:** `grid[0,0]`, `grid[0,1]`, `grid[1,0]` become null.
    *   **Logs:**
        *   "CheckForChemicalBonds called for piece at [x, y]." (for one of the pieces involved).
        *   "GetAdjacentPieces returned 3 pieces..." (or more if other pieces are touching this group).
        *   "Checking for bonds between pieces: - Element type 0: 2 pieces - Element type 1: 1 pieces"
        *   (Potentially logs from `FindPossibleCombinations` showing recursion count).
        *   "Successful bond: Checked X combinations with ArePiecesConnected."
        *   "Chemical bond formed! Type: H2O"
        *   Logs for pieces being destroyed and `MakePiecesFall()` being called.
        *   Score update logs.

---

**BOND_NaCl_01: Successful NaCl formation (1xNa, 1xCl, connected).**
*   **Objective:** Verify NaCl bond (1 Sodium, 1 Chlorine) forms and removes pieces.
*   **Setup:**
    1.  `[0,0]`: Piece Na (type 2)
    2.  `[0,1]`: Piece Cl (type 3)
*   **Steps:**
    1.  Ensure the pieces are dropped adjacently.
*   **Expected Outcome:**
    *   **Visual:** Na and Cl pieces are removed.
    *   **Grid Data:** `grid[0,0]`, `grid[0,1]` become null.
    *   **Logs:**
        *   Similar to H₂O: "CheckForChemicalBonds...", "GetAdjacentPieces returned 2 pieces...",
        *   "Checking for bonds between pieces: - Element type 2: 1 pieces - Element type 3: 1 pieces"
        *   "Chemical bond formed! Type: NaCl"
        *   Piece removal and fall logs. Score update.

---

**BOND_CO2_01: Successful CO₂ formation (1xC, 2xO, connected).**
*   **Objective:** Verify CO₂ bond (1 Carbon type 5, 2 Oxygen type 1) forms.
*   **Setup:**
    1.  `[0,0]`: Piece O (type 1)
    2.  `[0,1]`: Piece C (type 5)
    3.  `[0,2]`: Piece O (type 1) OR `[1,1]`: Piece O (type 1) to ensure C is connected to both O.
*   **Steps:**
    1.  Drop pieces to achieve the connected configuration.
*   **Expected Outcome:**
    *   **Visual:** 1 C and 2 O pieces removed.
    *   **Grid Data:** Relevant cells become null.
    *   **Logs:**
        *   "Checking for bonds between pieces: - Element type 5: 1 pieces - Element type 1: 2 pieces"
        *   "Chemical bond formed! Type: CO2"
        *   Other logs similar to H₂O.

---

**BOND_NH3_01: Successful NH₃ formation (1xN, 3xH, connected).**
*   **Objective:** Verify NH₃ bond (1 Nitrogen type 4, 3 Hydrogen type 0) forms.
*   **Setup:**
    1.  `[0,1]`: Piece N (type 4)
    2.  `[0,0]`: Piece H (type 0)
    3.  `[1,1]`: Piece H (type 0)
    4.  `[0,2]`: Piece H (type 0)
    (Ensure N is connected to all three Hs).
*   **Steps:**
    1.  Drop pieces to achieve the connected configuration.
*   **Expected Outcome:**
    *   **Visual:** 1 N and 3 H pieces removed.
    *   **Grid Data:** Relevant cells become null.
    *   **Logs:**
        *   "Checking for bonds between pieces: - Element type 4: 1 pieces - Element type 0: 3 pieces"
        *   "Chemical bond formed! Type: NH3"
        *   Other logs similar to H₂O.

---

**BOND_GEN_01: Elements present for a bond but NOT connected – ensure no bond forms.**
*   **Objective:** Verify that pieces do not form a bond if they meet element requirements but are not physically connected as per `ArePiecesConnected`.
*   **Setup:**
    1.  For H₂O:
        *   `[0,0]`: Piece H (type 0)
        *   `[0,2]`: Piece O (type 1) (Note the gap at `[0,1]`)
        *   `[2,0]`: Piece H (type 0)
*   **Steps:**
    1.  Drop the pieces in these disconnected positions.
    2.  Trigger `CheckForChemicalBonds` for one of these pieces (e.g., by dropping an unrelated piece nearby that doesn't cause a fall affecting these).
*   **Expected Outcome:**
    *   **Visual:** No pieces are removed.
    *   **Grid Data:** Pieces remain in place.
    *   **Logs:**
        *   "CheckForChemicalBonds called..."
        *   `GetAdjacentPieces` might return only 1 piece if they are truly isolated, or more if they are part of separate small clusters.
        *   If `GetAdjacentPieces` does manage to return all 3 pieces (e.g. if its definition of "adjacent" is very broad and they are somehow linked by other pieces not part of the H2O recipe), then the critical part is:
            *   `FindPossibleCombinations` might find the H, H, O combination.
            *   The call to `ArePiecesConnected(combination)` for this H,H,O set should return `false`.
            *   "No valid bond pattern found for these pieces" or similar, for this specific attempt. No "Chemical bond formed!" log for H2O.

---

**BOND_GEN_02: Some elements for a bond missing – ensure no bond forms.**
*   **Objective:** Verify that a bond does not form if not all required element types/counts are present.
*   **Setup:**
    1.  For H₂O:
        *   `[0,0]`: Piece H (type 0)
        *   `[0,1]`: Piece O (type 1)
        *   (Missing the second H)
*   **Steps:**
    1.  Drop these pieces.
*   **Expected Outcome:**
    *   **Visual:** No pieces removed.
    *   **Grid Data:** Pieces remain.
    *   **Logs:**
        *   "CheckForChemicalBonds called..."
        *   "Checking for bonds between pieces: - Element type 0: 1 pieces - Element type 1: 1 pieces" (or similar, showing insufficient H).
        *   The loop `foreach (var bond in chemicalBonds)` will iterate. For the H2O recipe, the condition `if (!elementCounts.ContainsKey(element.Key) || elementCounts[element.Key] < element.Value)` should be true (missing H, or not enough H).
        *   No "Chemical bond formed!" log for H₂O.

---

**BOND_GEN_03: Multiple potential bonds possible simultaneously – document observed behavior.**
*   **Objective:** Observe which bond forms if pieces are arranged to satisfy multiple bond types simultaneously.
*   **Setup (Example - H₂O and NaCl):**
    1.  This requires careful placement. Imagine a central piece that could be part of two bonds. This is hard with current small recipes.
    2.  Alternative: Two separate, valid bond formations existing on the grid at the same time.
        *   H₂O set: `[0,0]=H, [0,1]=O, [1,1]=H`
        *   NaCl set: `[3,0]=Na, [3,1]=Cl`
*   **Steps:**
    1.  Drop the final piece that completes *both* formations, or drop a piece that triggers `CheckForChemicalBonds` in a region that covers both.
    2.  If one piece drop completes two potential bonds involving that piece, this is a more direct test. E.g. Piece X is dropped, it completes Bond A with neighbors Y,Z and also Bond B with neighbors P,Q.
*   **Expected Outcome:**
    *   **Visual:**
        *   If bonds are non-overlapping, both should form and pieces removed.
        *   If bonds overlap (share pieces), one will likely form, consume its pieces, and then the conditions for the second bond may no longer be met. The current code iterates through `chemicalBonds` dictionary. The first recipe in that dictionary that can be formed with the available pieces and passes `ArePiecesConnected` will be made. The `CheckForChemicalBonds` method then returns after the first successful bond and subsequent `MakePiecesFall`.
    *   **Logs:**
        *   Logs will show which bond was identified and formed first.
        *   "Chemical bond formed! Type: {BondType}" for the first one.
        *   If pieces fall, `MakePiecesFall` is called, then it re-evaluates. It's possible a second bond could form after the first set of pieces are removed and pieces fall *if* the `locationsToCheck` logic correctly identifies the area for the second potential bond.

---

**BOND_GEN_04: Logs confirm appropriate `GetAdjacentPieces` count and `FindPossibleCombinations` recursion depth for a typical successful bond.**
*   **Objective:** Use logs to understand the typical workload for forming a simple bond.
*   **Setup:**
    1.  Use the H₂O setup from BOND_H2O_01: `[0,0]=H, [0,1]=O, [1,0]=H`.
*   **Steps:**
    1.  Form the bond.
*   **Expected Outcome:**
    *   **Logs:**
        *   "GetAdjacentPieces returned 3 pieces..." (assuming no other pieces are touching this H₂O group).
        *   "CheckForChemicalBonds at [x,y] finished. Total FindPossibleCombinations calls: {count}" - Record this count. For 3 pieces and a simple H2O recipe, this count should be relatively small (e.g., < 20). A high number would indicate inefficiency in `FindPossibleCombinations` or `IsValidCombination`.
        *   "Successful bond: Checked 1 combinations with ArePiecesConnected." (Assuming the H2O combination is found quickly).

---

## C. Edge Case Tests

**EDGE_01: Grid becomes full, game over is triggered.**
*   **Objective:** Verify game over occurs when no more pieces can be placed on the grid.
*   **Setup:**
    1.  Start the game.
*   **Steps:**
    1.  Keep dropping pieces, filling columns one by one, until the entire grid (`width` x `height`) is full of pieces.
    2.  Attempt to drop one more piece after the grid is visually full.
*   **Expected Outcome:**
    *   **Visual:** The piece cannot be placed. A "Game Over" screen or message should appear. (Based on `UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");`)
    *   **Grid Data:** All `grid[c,r]` cells should be non-null.
    *   **Logs:**
        *   When `DropPiece()` is called and `FindLowestEmptyCell` for the target column returns -1 for all columns, the `IsGridFull()` method should return true.
        *   "Game Over - Grid is full!"
        *   Log indicating "GameOverScene" is being loaded.

---

**EDGE_02: Column becomes full, piece cannot be dropped in that column.**
*   **Objective:** Verify a piece cannot be dropped in a column that is already full.
*   **Setup:**
    1.  Start the game.
*   **Steps:**
    1.  Fill one column completely (e.g., column 0 from `[0,0]` to `[0,height-1]`).
    2.  Ensure the `currentFallingPiece` is controlled by the player and is above this full column (column 0).
    3.  Attempt to drop the piece by pressing Enter/Return.
*   **Expected Outcome:**
    *   **Visual:** The `currentFallingPiece` does not get placed in the full column. It should remain under player control, allowing them to move it to another column.
    *   **Grid Data:** Column 0 remains unchanged (still full). The `currentFallingPiece` is not added to `grid[,]`.
    *   **Logs:**
        *   `DropPiece()` is called.
        *   `FindLowestEmptyCell(0)` (for column 0) returns -1.
        *   "Found empty cell at row -1 in column 0" (or similar based on exact logging for -1).
        *   "Column 0 is full!"
        *   `SpawnNewPiece()` should *not* be called if the piece wasn't successfully dropped. The player should retain control of `currentFallingPiece`.
```
