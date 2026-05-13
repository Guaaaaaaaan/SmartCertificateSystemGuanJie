using SmartCertificateSystem.Models;

namespace SmartCertificateSystem.Utilities;

public class GpaCalculator
{
    public double CalculateGpa(IEnumerable<Grade> grades)
    {
        var gradeList = grades.ToList();
        if (gradeList.Count == 0)
        {
            return 0;
        }

        var totalCredits = gradeList.Sum(g => Math.Max(1, g.CreditValue));
        var weightedPoints = gradeList.Sum(g => CalculateGradePoint(g.Score) * Math.Max(1, g.CreditValue));

        return Math.Round(weightedPoints / totalCredits, 2);
    }

    public string GetLetterGrade(double score) => score switch
    {
        >= 85 => "A",
        >= 75 => "B+",
        >= 65 => "B",
        >= 55 => "C+",
        >= 50 => "C",
        _ => "F"
    };

    public double CalculateGradePoint(double score) => score switch
    {
        >= 85 => 4.0,
        >= 75 => 3.5,
        >= 65 => 3.0,
        >= 55 => 2.5,
        >= 50 => 2.0,
        _ => 0
    };
}
