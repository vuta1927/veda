export interface IDashboard{
    project: projectAnalist;
}

export class projectAnalist{
    totalImages: number = 0;
    imagesTagged: number = 0;
    imagesHadQc: number = 0;
    totalTaggedTime: string = '0 hours 0 minutes';
    totalTags: number = 0;
    totalTagsHaveClass: number = 0;
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
    taggedTime: number;
    totalTags: number;
    totalQcs: number;
}