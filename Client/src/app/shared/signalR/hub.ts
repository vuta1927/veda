
import { HubConnection } from '@aspnet/signalr';

export class hub{
    public static _hubConnection: HubConnection;
    public static connectionId: string;
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
            alert(data.message);
            this._hubConnection.stop();
        });
    }
}