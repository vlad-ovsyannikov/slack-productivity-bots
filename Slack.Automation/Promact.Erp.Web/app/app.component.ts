﻿
import { Component, OnInit } from '@angular/core';
import { LoaderService } from './shared/loader.service';
import { AppComponentService } from './appcomponent.service';
import { EmailHashCode } from './shared/emailHashCode';

@Component({
    selector: 'my-app',
    templateUrl: './app/index.html'

})

export class AppComponent implements OnInit {
    userIsAdmin: boolean;
    hashCode: string;
    constructor(private loader: LoaderService, private httpService: AppComponentService, private emailHashCode: EmailHashCode) {
        this.userIsAdmin = false;
    }

    ngOnInit() {
        this.hashCode = this.emailHashCode.hashCode;
        this.httpService.getUserIsAdminOrNot().then((result) => {
            this.userIsAdmin = (result === 'true');
        });
    }
}