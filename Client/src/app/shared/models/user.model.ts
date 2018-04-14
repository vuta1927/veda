import './role.model'
import { IRole } from './role.model';
export interface IUser {
    userName: string;
    normalizedUserName: string;
    email: string;
    name: string;
    surname: string;
    accessFailedCount: number;
    isLockoutEnabled: boolean;
    lockoutEndDateUtc: Date;
    normalizedEmail: string;
    passwordHash: string;
    securityStamp: string;
    emailConfirmed: boolean;
    roles: IRole[];
    isActive: boolean;
    id: number
}

export interface IUserEdit {
    id?: number;
    name: string;
    surname: string;
    username: string;
    emailAddress: string;
    password: string;
    isActive: boolean;
    shouldChangePasswordOnNextLogin: boolean;
}
export interface IUserRole {
    roleId: number;
    roleName: string;
    roleDisplayName: string;
    isAssigned: boolean;
}

export interface IUserForCreateOrEdit {
    assignedRoleCount: number;
    isEditMode: boolean;
    user: IUserEdit;
    roles: IUserRole[];
}

export class CreateOrUpdateUser {
    user: IUserEdit;
    assignedRoleNames: string[];
    sendActivationEmail: boolean;
}