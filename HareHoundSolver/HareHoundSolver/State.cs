using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Navigation;

namespace HareHoundSolver
{
    // Since "hare" and "hound" share the same first letter, we will use
    // the terms "dog" and "rabbit" instead.
    //
    // Game states uniquely corresponds to IDs, which are encoded from
    // the board state and next player.
    //
    // Cell ID:
    //      0   1   2
    //  3   4   5   6   7
    //      8   9  10
    // Board state ID: (dog 0's position) * 11^3 + (dog 1's position) * 11^2 +
    //                 (dog 2's position) * 11 + (rabbit's position)
    // Boards where (dog 0's position < dog 1's position < dog 2's position) is not satisfied
    // are invalid.
    // Next payer ID: dog -- 0 | rabbit -- 1
    //
    // Game state ID: (board state) *2 + (next player ID)
    class State
    {
        public State(int id)
        {
            this.id = id;
            prev_states = new List<int>();
            next_states = new List<int>();
            expected_outcome = Outcome.Undecided;
            Initialize();
        }

        public int id;
        // Inclusive.
        public const int max_board_id = 2 * (8 * 1331 + 9 * 121 + 10 * 11 + 7);
        // Inclusive.
        public const int max_id = max_board_id * 2 + 1;

        public List<int> prev_states;
        public List<int> next_states;
        public List<State> NextStates { get; set; }

        private void Initialize()
        {
            // Decode ID.
            next_player = (NextPlayer)(id % 2);
            int board_state = id / 2;
            rabbit_position = board_state % 11;
            board_state /= 11;
            dog_positions = new int[3];
            for (int i = 2; i >= 0; i--)
            {
                dog_positions[i] = board_state % 11;
                board_state /= 11;
            }

            // Is this state valid?
            valid = true;
            if (dog_positions[0] >= dog_positions[1])
            {
                valid = false;
                return;
            }
            if (dog_positions[1] >= dog_positions[2])
            {
                valid = false;
                return;
            }
            for (int i = 0; i < 3; i++)
            {
                if (rabbit_position == dog_positions[i])
                {
                    valid = false;
                    return;
                }
            }

            // Is there an outcome?
            outcome = Outcome.Undecided;
            // The dogs win if the rabbit's position only has 3 neighbors, and
            // dogs occupy all of them.
            int[] rabbit_neighbors = Neighbors(rabbit_position);
            if (rabbit_neighbors.Length == 3 &&
                dog_positions[0] == rabbit_neighbors[0] &&
                dog_positions[1] == rabbit_neighbors[1] &&
                dog_positions[2] == rabbit_neighbors[2])
            {
                outcome = Outcome.DogsWin;
            }
            // The rabbit wins if no dog is to the left of it.
            else
            {
                int rabbit_column = Column(rabbit_position);
                if (Column(dog_positions[0]) >= rabbit_column &&
                    Column(dog_positions[1]) >= rabbit_column &&
                    Column(dog_positions[2]) >= rabbit_column)
                {
                    outcome = Outcome.RabbitWins;
                }
            }

            // If no outcome, find next states.
            // Previous states will be filled by MainWindow.
            if (outcome != Outcome.Undecided)
            {
                expected_outcome = outcome;
                return;
            }
            switch (next_player)
            {
                case NextPlayer.Dogs:
                    for (int dog = 0; dog < 3; dog++)
                    {
                        int[] dog_neighbors = Neighbors(dog_positions[dog]);
                        foreach (int next_dog in dog_neighbors)
                        {
                            if (Column(next_dog) <
                                Column(dog_positions[dog]))
                            {
                                // Dogs can't go left.
                                continue;
                            }
                            int[] next_dogs = new int[]
                            {
                                dog_positions[0],
                                dog_positions[1],
                                dog_positions[2]
                            };
                            next_dogs[dog] = next_dog;
                            int next_state = EncodeState(
                                next_dogs, rabbit_position, NextPlayer.Rabbit);
                            if (next_state != -1) next_states.Add(next_state);
                        }
                    }
                    break;
                case NextPlayer.Rabbit:
                    foreach (int next_rabbit in rabbit_neighbors)
                    {
                        int next_state = EncodeState(
                            dog_positions, next_rabbit, NextPlayer.Dogs);
                        if (next_state != -1) next_states.Add(next_state);
                    }
                    break;
            }
            next_states.Sort();
        }

        private static int[] Neighbors(int position)
        {
            switch (position)
            {
                case 0:
                    return new int[] { 1, 3, 4, 5};
                case 1:
                    return new int[] { 0, 2, 5 };
                case 2:
                    return new int[] { 1, 5, 6, 7 };
                case 3:
                    return new int[] { 0, 4, 8 };
                case 4:
                    return new int[] { 0, 3, 5, 8 };
                case 5:
                    return new int[] { 0, 1, 2, 4, 6, 8, 9, 10 };
                case 6:
                    return new int[] { 2, 5, 7, 10 };
                case 7:
                    return new int[] { 2, 6, 10 };
                case 8:
                    return new int[] { 3, 4, 5, 9 };
                case 9:
                    return new int[] { 5, 8, 10 };
                case 10:
                    return new int[] { 5, 6, 7, 9 };
                default:
                    return new int[] { };
            }
        }

        private static int Column(int position)
        {
            switch (position)
            {
                case 3:
                    return 0;
                case 0:
                case 4:
                case 8:
                    return 1;
                case 1:
                case 5:
                case 9:
                    return 2;
                case 2:
                case 6:
                case 10:
                    return 3;
                case 7:
                    return 4;
                default:
                    return -1;
            }
        }

        // Dog positions can be out of order; they will be sorted.
        // Returns -1 on invalid input.
        private static int EncodeState(int[] dogs, int rabbit, NextPlayer next_player)
        {
            Array.Sort(dogs);
            if (dogs[0] == dogs[1]) return -1;
            if (dogs[1] == dogs[2]) return -1;
            if (dogs[0] == rabbit) return -1;
            if (dogs[1] == rabbit) return -1;
            if (dogs[2] == rabbit) return -1;
            int board_state = dogs[0] * 1331 + dogs[1] * 121 + dogs[2] * 11 + rabbit;
            return board_state * 2 + (int)next_player;
        }

        // Draw games only happen after 30 moves, which this class does not track.
        public enum Outcome
        {
            Undecided = -1,
            DogsWin = 0,
            RabbitWins = 1
        }
        public Outcome outcome;
        // This is the expected outcome from the current state, assuming
        // both players play best moves.
        public Outcome expected_outcome;

        // Below are decoded from ID. We have enough memory to store them so why not.

        public int[] dog_positions;
        public int rabbit_position;
        public bool valid;
        public enum NextPlayer
        {
            Dogs = 0,
            Rabbit = 1
        }
        public NextPlayer next_player;

        public string Display
        {
            get
            {
                if (!valid)
                {
                    return $"Invalid state #{id}";
                }

                char[] board_display = { '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-'};
                for (int i = 0; i < 3; i++)
                {
                    board_display[dog_positions[i]] = 'D';
                }
                board_display[rabbit_position] = 'R';

                string display = "";

                // Line 1: row 1, state #
                display += ' ';
                for (int i = 0; i < 3; i++) display += board_display[i];
                display += ' ';
                display += $"  #{id}\n";

                // Line 2: row 2, next player
                for (int i = 3; i < 8; i++) display += board_display[i];
                display += $"  Next player: {next_player}\n";

                // Line 3: row 3, expected outcome
                display += ' ';
                for (int i = 8; i < 11; i++) display += board_display[i];
                display += ' ';
                display += $"  Expected outcome: {expected_outcome}";

                return display;
            }
        }
    }
}
