using System.ComponentModel.DataAnnotations;
using wnc.Models;

namespace wnc.Features.Admin.Rounds.ViewModels;

public class DocumentTypeSearchViewModel
{
    // Tiêu chí 1: Tìm theo Mã giấy tờ (DocumentCode)
    public string Keyword1 { get; set; } 

    // Tiêu chí 2: Tìm theo Tên giấy tờ (DocumentName)
    [Required(ErrorMessage = "Vui lòng nhập tiêu chí thứ 2.")]
    [RegularExpression(@"^[\p{L}\s]{3,}$", ErrorMessage = "Tiêu chí này phải có ít nhất 3 chữ cái và không được chứa số.")]
    public string Keyword2 { get; set; } 

    public List<DocumentType> Results { get; set; } = new List<DocumentType>();
}