﻿import { Injectable } from '@angular/core';
import { Http, Headers, RequestOptions, Response, URLSearchParams } from "@angular/http";
import { Observable } from 'rxjs/Rx';
import { StringConstant } from '../shared/stringConstant';
import { GroupModel } from './group.model';

@Injectable()
export class GroupService {
    private headers = new Headers({ 'Content-Type': 'application/json' });
    constructor(private http: Http, private stringConstant: StringConstant) {

    }

    /*This service used for get group list*
   *
   */
    getListOfGroup(): Promise<GroupModel[]> {
        return this.http.get(this.stringConstant.groupUrl).map(res => res.json())
            .toPromise();
    }

    /*This service used for add new group*
     * 
     * @param groupModel
     */
    addGroup(groupModel: GroupModel) {
        return this.http.post(this.stringConstant.groupUrl, JSON.stringify(groupModel), { headers: this.headers })
            .toPromise();
    }

    /*This service used for get group by id*
     * 
     * @param id
     */
    getGroupbyId(id: number) {
        return this.http.get(this.stringConstant.groupUrl + '/' + id).map(res => res.json())
            .toPromise();
    }

    /*This service used for update group*
     * 
     * @param groupModel
     */
    updateGroup(groupModel: GroupModel) {
        let id = + groupModel.Id;//convert to int 
        return this.http.put(this.stringConstant.groupUrl + '/' + id, JSON.stringify(groupModel), { headers: this.headers })
            .toPromise();

    }

    /*This service used for check group name is already exists or not*
     * 
     * @param groupName
     * @param id
     */
    checkGroupNameIsExists(name: string, id: number) {
        let params = new URLSearchParams();
        params.set(this.stringConstant.groupName, name);
        return this.http.get(this.stringConstant.groupUrl + '/available/' + id, { search: params }).map(res => res.json())
            .toPromise();
    }

    /*This service used for delete group by id.*
     * 
     * @param id
     */
    deleteGroupById(id: number) {
        return this.http.delete(this.stringConstant.groupUrl + '/delete' + '/' + id).map(res => res.json())
            .toPromise();
    }

    /*This service used for get active user email list*
     * 
     */
    getActiveUserEmailList() {
        return this.http.get(this.stringConstant.groupUrl + '/email').map(res => res.json())
            .toPromise();
    }

}

