export interface IProject{
    id: string;
    name: string;
    description: string;
}

export class ProjectForView implements IProject{
    constructor(public id:string = null,public name: string = null,public description: string = null, public note: string = null, public totalImg: number = 0,public totalImgNotTagged: number = 0, public totalImgNotClassed: number = 0, public totalImgQC: number = 0, public totalImgNotQC: number = 0, public usernames: string = null, public isDisable: boolean = false){}
}

export class ProjectForAdd implements IProject{
    constructor(public id:string,public name: string,public description: string = null, public note: string = null){}
}

export class ProjectForUpdate{
    constructor(public id:string,public name: string,public description: string = null, public note: string = null){}
}