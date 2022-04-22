import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-fetch-user',
  templateUrl: './fetch-user.component.html',
  styleUrls: ['./fetch-user.component.css']
})
export class FetchUserComponent {
  public userInfo?: UserInfo;

  constructor(http: HttpClient ) {
    var baseUrl = environment.baseUrl;

    http.get<UserInfo>(baseUrl + 'weatherforecast/getuserinfo').subscribe({
      next: (result) => this.userInfo = result,
      error: (error) => console.error(error)
    });
  }
}

interface UserInfo {
  id: string;
  claims: Claim[];
}

interface Claim {
  key: string;
  value: string;
}
