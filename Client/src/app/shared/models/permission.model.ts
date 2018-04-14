export interface IPermission {
    id: number
    name: string;
    displayName: string;
    category: string;
    descriptions: string;
}

export class Permission implements IPermission{
    constructor(public id: number, public parent: Permission, public children: Permission[], public name:string, public descriptions: string, public category: string, public displayName: string){}
}

export class PermissionForView implements IPermission{
    constructor(public id: number, public name:string, public descriptions: string, public category: string, public displayName: string){}
}

export class PermissionCategory implements IPermission{
    constructor(public id: number, public name:string, public descriptions: string, public category: string, public displayName: string, public isCheck: boolean){}
}