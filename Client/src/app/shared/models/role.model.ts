import { IUser } from './user.model';
import { RoleClaim } from './roleClaim.model';
import { Permission, PermissionForView, PermissionCategory } from './permission.model';
export interface IRole {
    id: number
    roleName: string;
    descriptions: string;
}

export class Role implements IRole{
    constructor(public id: number, public roleName: string, public normalizedRoleName: string, public permissions: Permission[],public lastModifierUser: IUser, public deleterUser: IUser, public isDeleted: boolean, public deleterUserId: number, public deletionTime: Date, public lastModificationTime: Date, public lastModifierUserId: number, public creationTime:Date, public creatorUserId:number, public creatorUser: IUser, public descriptions: string){
    }
}

export class RoleForView implements IRole{
    constructor(public id: number, public roleName: string, public descriptions: string, public permissions: PermissionCategory[]){

    }
}

export class RoleForUpdate implements IRole{
    constructor(public id: number, public roleName: string, public descriptions: string, public lastModifierUserId: any){

    }
}

export class RoleForCreate implements IRole{
    constructor(public id: number, public roleName: string, public descriptions: string, public creatorUserId: any){

    }
}