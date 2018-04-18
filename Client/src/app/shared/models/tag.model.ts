export interface ITag{
    id: number;
    imageId: string;
    classIds: number[];
    quantityCheckId: number;
    top: number;
    left: number;
}

export interface ITagForAdd{
    imageId:number;
    classId:number[];
    top: number;
    left: number;
}

export interface ITagForUpdate{
    id: number;
    imageId:number;
    classId:number[];
    top: number;
    left: number;
}

export class Tag implements ITag{
    constructor(public id: number = -1, public imageId: string, public classIds: number[] = [], public quantityCheckId: number = -1, public top: number, public left: number){

    }
}