1. High-level concept

Goal:
Use achievements and timed feature unlocks across the first 7 days to:

Teach pattern types (pairs, runs, suits, colors).

Teach hand management (draw/discard, wilds).

Introduce Run Mode and competitive stats after core mastery.

Lean on small, frequent achievements Days 1–3, then bigger, run/leaderboard-based goals from Day 3 onward.

You’re basically scripting the player’s “first week story”:

Learn → Feel clever → Discover Run Mode → Chase personal bests → Notice leaderboards → Care about rewards.

2. Player flow – Day-by-day progression
Day 1 – “Learn the basics”

Features available:

Normal Level Mode (Levels 1–10).

Only Pairs and Short Runs (3 cards) in early levels.

Achievements surfaced:

ACH_FIRST_LEVEL – “First Steps”

Trigger: Complete Level 1.

Reward: 50 coins.

ACH_FIRST_PAIR – “Pair Apprentice”

Trigger: Play your first valid pair.

Reward: 25 coins.

ACH_FIRST_RUN_3 – “Run Rookie”

Trigger: Play your first run of 3.

Reward: 25 coins.

ACH_COMPLETE_3_LEVELS – “On a Roll”

Trigger: Reach Level 3.

Reward: small cosmetic (basic card back A).

Guidance:

Early levels have one pattern type each:

Level 1: Pairs only.

Level 2: Runs only.

Level 3: Mix of pair + run (but very forgiving).

After Level 3, show a small modal: “Great! You’ve learned the basics. Tomorrow, we’ll show you more advanced patterns.”

Day 2 – “Introduce suits & colors”

Features available:

Levels 4–10 unlocked.

Suit sets and/or color sets appear in goals.

Discard pile starts to matter more (deck is tighter).

Achievements surfaced (auto-visible from Day 2 onward):

ACH_FIRST_SUIT_SET – “Suit Up”

Trigger: Play a pattern of 3+ cards of the same suit.

Reward: 25 coins.

ACH_FIRST_COLOR_SET – “Color Coder”

Trigger: Play 3+ cards of same color.

Reward: 25 coins.

ACH_USE_DISCARD_5_TIMES – “Second Thoughts”

Trigger: Draw from discard pile 5 times total.

Reward: 50 coins.

ACH_WIN_WITH_MOVES_LEFT – “Efficient Planner”

Trigger: Win a level with ≥3 moves remaining.

Reward: 25 coins.

Guidance:

A Day 2 login popup:

“New tricks unlocked! Now you can score with suit and color patterns. Try combining them with your runs and pairs.”

Level design nudges:

Have at least one level where suit sets are strictly better than pairs for hitting goals (higher score / fewer cards).

Day 3 – “Unlock Run Mode & basic competition”

Gate to unlock Run Mode:

Condition: AccountAgeDays >= 2 AND HighestLevelCompleted >= 5.

Features unlocked:

Run Mode (simple streak of levels).

Personal best tracking.

A small “local” leaderboard (even if just dummy/bots in soft launch).

Achievements surfaced (Run-related):

ACH_FIRST_RUN_MODE_LEVEL – “Taking a Run”

Trigger: Complete your first level in Run Mode.

Reward: 50 coins.

ACH_RUN_3_LEVELS – “Streak of Three”

Trigger: Complete 3 levels in one run.

Reward: 100 coins.

ACH_VIEW_LEADERBOARD – “Sizing Up the Competition”

Trigger: Open the leaderboard screen.

Reward: 25 coins.

ACH_PERSONAL_BEST_IMPROVED – “New Personal Best”

Trigger: Beat your own best run-length or best score.

Reward: small cosmetic (profile border A).

Guidance:

Day 3 login popup:

“Run Mode Unlocked! Try playing consecutive levels and see how far you can go in one streak. Compare your run with others!”

After first run-level:

Show quick summary: “Run 1: Levels completed: 1 | Best: 1”
And a button: [View Leaderboard].

Day 4 – “Teach combos & big hands”

Features emphasized:

Longer runs (4–5 cards).

Multi-pattern chains (e.g., playing a run then a pair in back-to-back turns).

Achievements surfaced:

ACH_RUN_4 – “Long Runner”

Trigger: Play a run of 4+ cards.

Reward: 50 coins.

ACH_TWO_PATTERNS_IN_A_ROW – “Combo Starter”

Trigger: Play a valid pattern on two consecutive turns in a single level.

Reward: 75 coins.

ACH_BEST_HAND_SCORE_200 – “Power Hand” (tune the threshold later)

Trigger: Score ≥ X points with a single hand play.

Reward: new card back B.

Guidance:

Levels on Day 4 (or ~Levels 8–12) should gently encourage holding back a card to create a larger run.

A tip on loading a level:

“Bigger patterns score more. Can you build a 4+ card run this level?”

Day 5 – “Introduce harder goals & mini-challenges”

Features emphasized:

High-difficulty goals in a few normal levels.

Optional “Challenge Level” or “Daily Challenge” appears.

Achievements surfaced:

ACH_WIN_HARD_LEVEL – “Hard Mode Rookie”

Trigger: Beat your first flagged “Hard” level.

Reward: 100 coins.

ACH_NO_HINTS_USED – “No Training Wheels”

Trigger: Complete a level without using hints.

Reward: small cosmetic (badge).

ACH_COMPLETE_DAILY_CHALLENGE – “Daily Grinder”

Trigger: Finish 1 Daily Challenge.

Reward: bonus coins + small progress towards a weekly chest.

Guidance:

Day 5 message:

“Feeling confident? Hard Levels and Daily Challenges are now available. They’re tougher, but the rewards are better.”

Daily Challenge:

One level per day, slightly constrained (fewer moves, trickier deck).

Great place to seed weekly quests later.

Day 6 – “Deepen Run Mode engagement”

Features emphasized:

Longer runs (5+ levels).

Run Mode summary UX.

Leaderboard competition.

Achievements surfaced:

ACH_RUN_5_LEVELS – “Marathoner”

Trigger: Complete 5 levels in one run.

Reward: 150 coins + rare title “Runner”.

ACH_RUN_TOP_100 – “Top 100”

Trigger: Reach rank ≤ 100 on Best Run Levels leaderboard (global or local).

Reward: exclusive card back C.

ACH_THREE_RUNS_IN_A_DAY – “Back for More”

Trigger: Start Run Mode 3 times in one calendar day.

Reward: small bonus (coins + progression toward some long-term meta reward).

Guidance:

Day 6 login popup:

“Your runs now count toward special rewards. Can you finish 5+ levels in a single run?”

Encourage sharing / bragging:

Optional “Share” button on the Run Summary.

Day 7 – “Mastery & long-term goals”

Features emphasized:

Long-tail achievements.

Weekly/seasonal progress idea.

Nudging player toward ongoing play beyond week 1.

Achievements surfaced:

ACH_RUN_10_LEVELS – “Endurance Master” (long-term goal, not necessarily hit on Day 7)

Trigger: Complete 10+ levels in one run.

Reward: high-tier cosmetic and lots of coins.

ACH_50_PATTERNS_TOTAL – “Pattern Pro”

Trigger: Play 50 patterns across all modes.

Reward: special profile title.

ACH_PLAY_7_DAYS – “First Week”

Trigger: Log in and play at least 1 level on 7 distinct days.

Reward: “Week One Chest” – bundle of coins + cosmetic + maybe a booster-type item (if you have those planned).

Guidance:

Day 7 message:

“You’ve completed your first week! Here’s a special chest to celebrate. Now chase longer runs, higher scores, and new cosmetics.”

From now on:

Focus on weekly events, new level packs, and seasonal goals.

3. Required data structures (C# / JSON)
3.1. Achievement definitions with gating

Extend your AchievementDefinition to include visibility & unlock conditions:

public class AchievementVisibilityCondition
{
    public int MinAccountDay;       // days since account creation/install
    public int MinLevelReached;     // min highest completed level
    public string RequiredFeatureFlag; // e.g. "RunMode", "HardMode", etc.
}

public class AchievementDefinition
{
    public string Id;
    public string Name;
    public string Description;
    public int TargetValue;
    public LeaderboardType? RelatedMetric;
    public List<RewardDefinition> Rewards;

    // New:
    public AchievementVisibilityCondition VisibilityCondition;
}

3.2. Player profile needs account age & feature flags
public class PlayerProfile
{
    public string PlayerId;
    public string DisplayName;
    public DateTime AccountCreatedAt;

    public int TotalCoins;
    public List<string> OwnedCosmetics;
    public string EquippedCardBack;
    public string EquippedTitle;

    public PersonalBestStats PersonalBests;
    public List<AchievementProgress> Achievements;

    // New:
    public int HighestLevelCompleted;
    public HashSet<string> UnlockedFeatures; // e.g., "RunMode", "HardMode", "DailyChallenge"
}


Helper:

public static class PlayerProfileExtensions
{
    public static int GetAccountAgeDays(this PlayerProfile profile, DateTime now)
    {
        return (now.Date - profile.AccountCreatedAt.Date).Days;
    }

    public static bool HasFeature(this PlayerProfile profile, string featureFlag)
    {
        return profile.UnlockedFeatures.Contains(featureFlag);
    }
}

3.3. Daily/first-week plan config

You can also explicitly store the “first week plan”:

public class DayPlan
{
    public int DayNumber;                   // 1..7
    public List<string> AchievementsToHighlight; // for UI emphasis
    public List<string> FeaturesToUnlock;        // feature flags
}

public class FirstWeekPlan
{
    public List<DayPlan> Days;
}


Example JSON for Day 3:

{
  "DayNumber": 3,
  "AchievementsToHighlight": [
    "ACH_FIRST_RUN_MODE_LEVEL",
    "ACH_RUN_3_LEVELS",
    "ACH_VIEW_LEADERBOARD"
  ],
  "FeaturesToUnlock": [
    "RunMode"
  ]
}


Your runtime can, on login:

Compute accountDay = GetAccountAgeDays(profile, now).

Apply any DayPlan with DayNumber == accountDay.

Activate those FeaturesToUnlock and show a popup highlighting the AchievementsToHighlight.

4. Edge cases & UX notes

Player speed differences

A very engaged player may reach Level 20 on Day 1.

Gating achievements by both account age and level ensures they don’t unlock Run Mode too early.

A very casual player might still be on Level 3 by Day 5.

They’ll see some achievements appear (like Daily Challenge once feature unlocked by day), but difficulty remains appropriate as it’s still tied to levels.

Avoid achievement spam

Limit to 2–3 easy achievements on Day 1.

Group multiple completions into a single summary banner if they trigger at once.

Clear communication of time-based stuff

“Play on 7 different days” should show progress like 5/7 with calendar-like markers.

Make “Day X unlocks” feel like rewards, not chores.

Feature discoverability

Whenever you unlock a feature (Run Mode, Hard Levels, Daily Challenge), show:

A quick tour or tooltip (1–2 steps max).

A button that jumps straight to that new mode.

Soft vs. hard gates

Try to never block the player from playing basic levels if they’re underleveled for some day’s content.

Use account-day gates mostly to delay surfacing complex systems (Run Mode, Hard, etc.), not to lock play.

If you’d like, next we can:

Lock in exact numeric thresholds for scoring and “Best Hand” achievements, or

Sketch the first 10 levels so they align perfectly with this first-week plan.

Sketch the first 10 levels so they align perfectly with this first-week plan.

Here’s a clean Level 1–10 sketch that lines up with the first-week plan and the systems we’ve already talked about.

I’ll treat this as Level Pack 1: “First Week”, tuned for a Rummy-style hand + draw/discard.

1. High-level concept

Goal of these 10 levels

Levels 1–3 (Day 1): Teach pairs and runs of 3.

Levels 4–7 (Day 2–3): Introduce suits and colors, reinforce basics.

Levels 8–10 (Day 3–5): Add longer runs, efficiency, and light challenge; prepare for Run Mode & Hard content.

Each level has:

A simple, memorable identity (“Pairs Only”, “Suit Up”, etc.).

A small set of AllowedPatterns.

Clear Goals that map 1:1 to early achievements.

2. Player flow across the first 10 levels

I’ll define a mini-spec per level:

Name / Theme

Target day (rough guidance; not a hard gate)

Allowed patterns

Goals

Deck & moves

Tutorial / UX notes

Level 1 – “Pairs Only”

Target day: Day 1 (first session)

AllowedPatterns: PAIR

Goals:

Make 3 pairs of any rank.

Deck & moves:

DeckSize: ~24 cards (12 ranks, each doubled).

No wilds, no suit/color relevance.

MoveLimit: ~12.

Tutorial notes:

Show Pairs tutorial overlay:

“Select two cards with the same rank to form a Pair.”

Ensure deck is guaranteed solvable (pre-baked order).

Very low chance to get stuck; high redundancy of pairs.

Level 2 – “First Runs”

Target day: Day 1

AllowedPatterns: RUN_3

Goals:

Make 2 runs of 3 (e.g., 2-3-4).

Deck & moves:

DeckSize: ~26 cards.

Ranks biased toward forming multiple 3-length sequences.

MoveLimit: ~14.

Tutorial notes:

Show Runs tutorial overlay:

“Select 3 cards in consecutive rank order to form a Run. Suits don’t matter here.”

No suit/color patterns yet; keep visual noise low (basic suits/colors, but not referenced).

Level 3 – “Mix & Match”

Target day: Day 1 (late)

AllowedPatterns: PAIR, RUN_3

Goals:

Make 1 Run of 3+.

Make 2 Pairs.

Deck & moves:

DeckSize: ~28 cards.

Enough overlapping ranks to let player choose between using cards for runs or pairs.

MoveLimit: ~16.

Tutorial notes:

Short tip:

“This level lets you use both Pairs and Runs. Choose patterns wisely to complete all goals.”

Designed to trigger:

ACH_FIRST_PAIR, ACH_FIRST_RUN_3, ACH_COMPLETE_3_LEVELS.

Very forgiving; early chance to feel clever about splitting decisions (e.g., “Do I use this 7 for a Pair now or hold for a Run?”).

Level 4 – “Suit Up”

Target day: Day 2

AllowedPatterns: PAIR, SUIT_3

Goals:

Make 1 Suit Set of 3+ cards (same suit).

Make 2 Pairs.

Deck & moves:

DeckSize: ~30 cards.

Suits tuned so that at least one suit appears 4–5 times.

MoveLimit: ~18.

Tutorial notes:

Show Suit tutorial overlay:

“New pattern: Suit Sets. Select 3+ cards of the same suit to score.”

Early risk: player may burn same-suit cards into pairs; keep deck forgiving so they can still complete suit goal.

Level 5 – “Color Coder”

Target day: Day 2

AllowedPatterns: PAIR, COLOR_3

Goals:

Make 1 Color Set of 3+ (e.g., all Red).

Make 2 Pairs.

Deck & moves:

DeckSize: ~30 cards.

Colors biased (e.g., lots of red/black) to make color sets easy.

MoveLimit: ~18.

Tutorial notes:

Show Color tutorial overlay:

“New pattern: Color Sets. Select 3+ cards of the same color.”

Good place to test colorblind-safe icons/textures.

Level 6 – “Your Choice”

Target day: Day 2–3

AllowedPatterns: PAIR, RUN_3, SUIT_3, COLOR_3

Goals:

Complete any 3 patterns (any combination).

Deck & moves:

DeckSize: ~32 cards.

Contains multiple overlapping opportunities:

e.g., some cards form both a run and a suit set.

MoveLimit: ~18.

Tutorial notes:

Emphasize freedom:

“Any valid pattern counts toward your goal. Use whichever patterns you like.”

Good level to show Hint for the first time.

Level 7 – “Efficient Planner”

Target day: Day 3

AllowedPatterns: RUN_3, SUIT_3, COLOR_3

Goals:

Complete 3 patterns total.

Soft design goal: encourage winning with moves left (for ACH_WIN_WITH_MOVES_LEFT).

Deck & moves:

DeckSize: ~32 cards.

MoveLimit: ~16 (still generous, but less than before).

More opportunities for larger patterns (4+ cards) to reward efficient planning.

Tutorial notes:

Pre-level tip:

“Try to plan ahead. Bigger patterns and fewer moves earn more points!”

Level is still “Normal”, but begins to feel like planning, not just reacting.

Level 8 – “Long Runner”

Target day: Day 4+

AllowedPatterns: RUN_3, RUN_4, SUIT_3

Goals:

Make 1 Run of 4+ cards (RUN_4).

Make 1 additional pattern of any allowed type.

Deck & moves:

DeckSize: ~34 cards.

At least one clear 4- or 5-length numeric chain in the deck.

MoveLimit: ~18.

Tutorial notes:

Tip:

“New goal: build longer runs for extra points. Sometimes it pays to wait for that extra card.”

This level is a natural trigger for ACH_RUN_4 (Long Runner).

Level 9 – “Combo Starter”

Target day: Day 4–5

AllowedPatterns: PAIR, RUN_3, SUIT_3, COLOR_3

Goals:

Complete 4 patterns in total.

Deck & moves:

DeckSize: ~34 cards.

MoveLimit: ~20, but with deck arranged so:

There are good opportunities to play patterns on consecutive turns (combos).

Tutorial notes:

Loading tip:

“Try chaining patterns on consecutive turns for huge scores!”

While combos are tracked in achievements (e.g., ACH_TWO_PATTERNS_IN_A_ROW), the level itself only cares about 4 patterns total, to stay simple.

Level 10 – “Mini Boss: Pattern Master”

Target day: Day 5+

AllowedPatterns: PAIR, RUN_3, RUN_4, SUIT_3, COLOR_3

Goals:

Make 1 Run of 4+.

Make 1 Suit Set of 3+.

Make 1 Color Set of 3+.

Deck & moves:

DeckSize: ~36 cards.

Introduce 1–2 wild cards to save frustration.

MoveLimit: ~22.

Tutorial notes:

Explicitly marked as a “Mini Boss”:

“This level tests everything you’ve learned: runs, suits, colors, and smart use of wilds.”

Great level to showcase:

Bigger score.

How different pattern types interact.