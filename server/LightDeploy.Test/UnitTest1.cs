using System.Text.RegularExpressions;
using LightDeploy.Core.Helper;

namespace LightDeploy.Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var currentFileInfos = FileHelper.GetFileInfos("E:\\PDM.WorkerService.Publish");
        var matched=currentFileInfos.Where(it => Regex.IsMatch(Path.Combine(it.RelativeDirectory, it.FileName), ".*UGApp.*"))
            .ToList();
        Assert.True(matched.Count>0);
        

    }
}