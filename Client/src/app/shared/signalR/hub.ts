
import { HubConnection } from '@aspnet/signalr';
import swal from 'sweetalert2';

export class hub{
    public static _hubConnection: HubConnection;
    public static connectionId: string;
    public constructor(){

    }
    public static setupHub(apiUrl) {
        var mother = this;
        this._hubConnection = new HubConnection(apiUrl + '/hubs/image');
        this._hubConnection
            .start()
            .then(() => {
                this.connectionId = this._hubConnection['connection'].connectionId;
            })
            .catch(err => console.log('Error while establishing connection (' + apiUrl + '/hubs/image' + ') !'));
        this._hubConnection.on("MergeNotification", data => {
            console.log(data);
            if(data.result){
                swal({
                    title: '',text: data.message, type: 'success'
                });
            }else{
                swal({
                    title: '',text: data.message, type: 'error'
                })
            }
            
            this._hubConnection.stop();
        });
    }
}