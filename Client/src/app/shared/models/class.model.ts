export interface IClass{
    id: number;
    code: string;
    name: string;
    description: string;
    totalTag: number;
    classColor: string;
}

export class Class implements IClass {
    constructor(
        public id:number = 0, 
        public code: string = null, 
        public name: string = null, 
        public description: string = null, 
        public totalTag: number =0, 
        public classColor:string='#000000'){

    }
}
export class ClassForAdd{
    projectId: string;
    code: string;
    name: string;
    description: string;
    classColor: string;
}

export class ClassForUpdate{
    id: number;
    code: string;
    name: string;
    description: string;
    classColor: string;
}

export class ClassList{
    constructor(
        public id:number, 
        public name: string, 
        public checked: boolean = false, 
        public classColor:string = '#000000'){}
}