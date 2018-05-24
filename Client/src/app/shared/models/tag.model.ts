export interface ITag{
    id: number;
    index: number;
    imageId: string;
    classId: number;
    quantityCheckId: number;
    top: number;
    left: number;
    width: number;
    height: number;
}

export interface ITagForAdd{
    imageId:number;
    classId:number;
    top: number;
    left: number;
    width: number;
    height: number;
}

export interface ITagForUpdate{
    id: number;
    imageId:number;
    classId:number;
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
        public classId: number, 
        public quantityCheckId: number = -1, 
        public top: number, 
        public left: number,
        public width: number,
        public height: number){

    }
}

export class TaggedTimeUser{
    constructor(
        public userId:number,
        public taggedTime: number
    ){ }
}

export class DataUpdate{
    constructor(
        public userId: number, 
        public tags:Tag[], 
        public taggedTime: number,
        public excluseAreas:ExcluseArea[],
        public ignored: boolean =false
    ){}
}

export class ExcluseArea {
    constructor(
        public name: string = null,
        public paths: Coordinate[] = []
    ){}
}

export class Coordinate{
    constructor(public x:number,public y:number){}
}