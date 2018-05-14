using ApiServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ApiServer.Core.MergeProject
{
    public static class Merge
    {
        public static VdsContext _ctx;

        private static Dictionary<int, int> IndexClassMap = new Dictionary<int, int>();

        public static async Task Execute(MergeModel.Merge mergeData)
        {
            var newProject = new Project();
            newProject.Name = mergeData.ProjectName;
            newProject.IsDisabled = false;
            newProject.TotalImg = newProject.TotalImgNotClassed = newProject.TotalImgNotQC = newProject.TotalImgNotTagged = newProject.TotalImgQC = 0;

            _ctx.Projects.Add(newProject);
            var newProjectUsers = new List<ProjectUser>();
            foreach (var user in mergeData.Users)
            {
                var u = _ctx.Users.SingleOrDefault(x => x.UserName == user.UserName);
                var role = _ctx.Roles.SingleOrDefault(x => x.RoleName == user.RoleName);
                if (u != null && role != null)
                {
                    _ctx.ProjectUsers.Add(new ProjectUser()
                    {
                        User = u,
                        UserId = u.Id,
                        Role = role,
                        RoleId = role.Id
                    });
                }
            }

            var newClasses = new List<Class>();
            foreach (var projId in mergeData.Projects)
            {
                var originClasses = _ctx.Classes.Include(x => x.Project).Where(x => x.Project.Id == projId);
                foreach (var klass in mergeData.Classes)
                {
                    if (klass.Id != 0)
                    {
                        var c = originClasses.SingleOrDefault(x => x.Id == klass.Id);
                        if (c != null)
                        {
                            newClasses.Add(new Class()
                            {
                                ClassColor = c.ClassColor,
                                Name = c.Name,
                                Note = c.Note,
                                Project = newProject,
                                Description = c.Description
                            });
                        }
                    }
                    else
                    {
                        newClasses.Add(new Class()
                        {
                            ClassColor = klass.ClassColor,
                            Name = klass.Name,
                            Project = newProject,
                            Description = klass.Description
                        });

                    }
                }
            }

            _ctx.Classes.AddRange(newClasses);

            await _ctx.SaveChangesAsync();
        }
    }
}
