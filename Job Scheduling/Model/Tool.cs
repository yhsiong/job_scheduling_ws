using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Job_Scheduling.Model
{
    public class Tool
    {
        [Key]
        public Guid tool_id { get; set; }
        public string tool_name { get; set; }
        public string tool_remark { get; set; }
        public string? tool_created_by { get; set; }
        public DateTime? tool_created_at { get; set; }
        public string? tool_updated_by { get; set; }
        public DateTime? tool_updated_at { get; set; }
        public string? tool_status { get; set; } 

        [NotMapped]
        public class Dto
        {
            public class Get : Tool
            {
            }
            public class Post : Tool
            {
            }
            public class Put : Tool
            {
            }
        }

        [NotMapped]
        public class Operations
        {
            public static async Task<bool> Create(Tool_Context tool_Context, Dto.Post toolScheme)
            {
                tool_Context.Tool.Add(toolScheme);
                try
                {
                    await tool_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            public static async Task<bool> Update(Tool_Context tool_Context, Dto.Put toolScheme)
            {
                Tool dtoTool = tool_Context.Tool.Where(x => x.tool_id.Equals(toolScheme.tool_id)).FirstOrDefault();
                dtoTool.tool_updated_by = toolScheme.tool_updated_by;
                dtoTool.tool_name = toolScheme.tool_name;
                dtoTool.tool_remark = toolScheme.tool_remark;
                dtoTool.tool_updated_at = toolScheme.tool_updated_at;
                dtoTool.tool_status = toolScheme.tool_status;

                tool_Context.Tool.Update(dtoTool);
                try
                {
                    await tool_Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            public static async Task<Dto.Get> ReadSingleById(Tool_Context tool_Context, Guid tool_id)
            {
                Tool tool = tool_Context.Tool.Where(a => a.tool_id.Equals(tool_id)).FirstOrDefault();

                if (tool == null)
                {
                    return null;
                }
                else
                {
                    return new Dto.Get
                    {
                        tool_id = tool.tool_id,
                        tool_name = tool.tool_name,
                        tool_created_at = tool.tool_created_at,
                        tool_created_by = tool.tool_created_by,
                        tool_remark = tool.tool_remark,
                        tool_status = tool.tool_status,
                        tool_updated_at = tool.tool_updated_at,
                        tool_updated_by = tool.tool_updated_by
                    };
                }
            }
            public static async Task<List<Dto.Get>> ReadAll(Tool_Context tool_Context)
            {
                List<Tool> tools = tool_Context.Tool.Where(a => !a.tool_status.Equals("Deleted")).ToList();

                if (tools == null)
                {
                    return null;
                }
                else
                {
                    return tools.Select(tool => new Dto.Get
                    { 
                        tool_id = tool.tool_id,
                        tool_name = tool.tool_name,
                        tool_created_at = tool.tool_created_at,
                        tool_created_by = tool.tool_created_by,
                        tool_remark = tool.tool_remark,
                        tool_status = tool.tool_status,
                        tool_updated_at = tool.tool_updated_at,
                        tool_updated_by = tool.tool_updated_by
                    }).ToList();

                }
            }
        }
    }
}
