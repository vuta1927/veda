export interface ITag{
    id: number;
    index: number;
    imageId: string;
    classIds: number[];
    quantityCheckId: number;
    top: number;
    left: number;
    width: number;
    height: number;
}

export interface ITagForAdd{
    imageId:number;
    classId:number[];
    top: number;
    left: number;
    width: number;
    height: number;
}

export interface ITagForUpdate{
    id: number;
    imageId:number;
    classId:number[];
    top: number;
    left: number;
    width: number;
    height: number;
}

export class Tag implements ITag{
    constructor(
        public id: number = -1, 
        public index: number,
        public imageId: string, 
        public classIds: number[] = [], 
        public quantityCheckId: number = -1, 
        public top: number, 
        public left: number,
        public width: number,
        public height: number){

    }
}

export class ExcluseArea {
    constructor(
        public path: Coodirnate[] = []
    ){}
}

export class Coodirnate{
    x: number;
    y: number;
}