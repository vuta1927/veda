import './role.model'
import { IRole } from './role.model';
export interface IProjectUser {
    id: number;
    userName: string;
    roleName: string;
}

export interface IProjectUserForMerge {
    id: number;
    userName: string;
    roleName: string;
    project: string;
}

export interface IProjectUserForAdd {
    id: string;
    userName: string;
    roleName: string;
}

export class ProjectUserForAdd implements IProjectUserForAdd {
    constructor(
        public id: string = null,
        public userName: string = null,
        public roleName: string = null) { }
}

