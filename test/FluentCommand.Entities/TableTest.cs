using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FluentCommand.Entities;

[Table("Table1 $ Test", Schema = "dbo")]
public class TableTest
{
    private const string BlahColumn = "Blah #";

    [Key]
    [Column("Test$")]
    public string Test { get; set; } = null!;

    [Column(BlahColumn)]
    public string Blah { get; set; }

    [Column("Table Example ID")]
    public int? TableExampleID { get; set; }

    public int? TableExampleObject { get; set; }

    [Column("1stNumber")]
    public string FirstNumber { get; set; }

    [Column("123Street")]
    public string Street { get; set; }

    [Column("123 Test 123")]
    public string Test123 { get; set; }
}
