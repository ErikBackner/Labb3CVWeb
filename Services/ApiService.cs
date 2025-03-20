using Labb3CVWeb.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Labb3CVWeb.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Project>> GetProjectsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Project>>("api/projects") ?? new List<Project>();
        }

        public async Task<Project?> GetProjectAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Project>($"api/projects/{id}");
        }

        public async Task<Project?> CreateProjectAsync(Project project)
        {
            var response = await _httpClient.PostAsJsonAsync("api/projects", project);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<Project>() : null;
        }

        public async Task<Project?> UpdateProjectAsync(int id, Project project)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/projects/{id}", project);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<Project>() : null;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/projects/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Skill>> GetSkillsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Skill>>("api/skills") ?? new List<Skill>();
        }

        public async Task<Skill?> GetSkillAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Skill>($"api/skills/{id}");
        }

        public async Task<Skill?> CreateSkillAsync(Skill skill)
        {
            var response = await _httpClient.PostAsJsonAsync("api/skills", skill);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<Skill>() : null;
        }

        public async Task<Skill?> UpdateSkillAsync(int id, Skill skill)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/skills/{id}", skill);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<Skill>() : null;
        }

        public async Task<bool> DeleteSkillAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/skills/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddSkillToProjectAsync(int projectId, int skillId)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/projects/{projectId}/addSkill", skillId);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveSkillFromProjectAsync(int projectId, int skillId)
        {
            var response = await _httpClient.DeleteAsync($"api/projects/{projectId}/removeSkill?skillId={skillId}");
            return response.IsSuccessStatusCode;
        }
    }
}