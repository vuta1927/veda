import { Class } from './class.model';

export class Merge {
    projectName: string = '';
    connectionId: string = '';
    classes: Class[] = [];
    mergeClasses: MergeKlass[] = [];
    projects: string[] = [];
    users: MergeProjectUser[] = [];
    filterOptions: FilterOptions = new FilterOptions();
}

export class MergeProjectUser{
    userName: string = '';
    roleName: string = '';
}

export class MergeKlass{
    oldClasses: Class[] = [];
    newClass: Class = new Class();
}

export class FilterOptions{
    qcOptions: QcOption[] = [];
}

export class QcOption{
    index: number;
    value: number;
}