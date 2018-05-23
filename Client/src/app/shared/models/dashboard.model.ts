export interface IDashboard{
    project: projectAnalist;
}

export class projectAnalist{
    totalProjects: number = 0;
    totalProjectImages: number = 0;
    totalImages: number = 0;
    imagesTagged: number = 0;
    imagesHadQc: number = 0;
    totalTaggedTime: string = '0 hours 0 minutes';
    totalTags: number = 0;
    totalTagsHaveClass: number = 0;
    currentProgress: UserProject = new UserProject();
    userProjects: UserProject[] = [];
}

export class project{
    id:string;
    name: string;
}

export class UserProject{
    userId: number;
    userName: string;
    email: string;
    roleNames: string[];
    taggedTime: string;
    totalTags: number;
    totalQcs: number;
    tagsHaveClass: number;
    imagesHaveTag: number;
    imagesHaveTagClass: number;
}