export interface ProjectSetting{
    id: number;
    taggTimeValue: number;
    quantityCheckLevel: number;
}

export class ProjectSettingForUpdate{
    taggTimeValue: number = 0;
    quantityCheckLevel: number = 0;
    projectId: string = '';
}