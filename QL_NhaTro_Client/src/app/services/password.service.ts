import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PasswordService {
  private REST_API_SERVER = 'http://localhost:5001/api';

  constructor(private http: HttpClient) {}

  changePassword(data: {
    currentPassword: string;
    newPassword: string;
  }): Observable<any> {
    return this.http.post(`${this.REST_API_SERVER}/password/change`, data);
  }
}
