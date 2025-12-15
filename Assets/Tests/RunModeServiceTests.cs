using NUnit.Framework;

public class RunModeServiceTests
{
    [Test]
    public void RunModeRecordsBestRunLength()
    {
        var profile = new PlayerProfile();
        profile.tutorialStep = ProgressionService.TutorialMaxStep + 1;

        // Replace cached profile so the service operates on this instance.
        ProgressionService.SetProfileForTests(profile);

        RunModeService.StartNewRun();
        Assert.IsTrue(RunModeService.IsActive);

        RunModeService.RecordLevelWin(10);
        RunModeService.RecordLevelWin(10);
        Assert.AreEqual(2, profile.currentRunLength);
        Assert.AreEqual(2, profile.bestRunLength);
    }
}
