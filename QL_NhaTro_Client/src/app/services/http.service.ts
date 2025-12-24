import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class HttpService {
  private readonly API_URL = 'http://localhost:5001/api';

  constructor(private http: HttpClient) {}

  // Getter để các service khác có thể truy cập API URL
  get apiUrl(): string {
    return this.API_URL;
  }

  // HTTP GET
  get<T>(endpoint: string, params?: HttpParams): Observable<T> {
    return this.http.get<T>(`${this.API_URL}/${endpoint}`, { params });
  }

  // HTTP POST
  post<T>(endpoint: string, body: any): Observable<T> {
    return this.http.post<T>(`${this.API_URL}/${endpoint}`, body);
  }

  // HTTP PUT
  put<T>(endpoint: string, body: any): Observable<T> {
    return this.http.put<T>(`${this.API_URL}/${endpoint}`, body);
  }

  // HTTP PATCH
  patch<T>(endpoint: string, body: any): Observable<T> {
    return this.http.patch<T>(`${this.API_URL}/${endpoint}`, body);
  }

  // HTTP DELETE
  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.API_URL}/${endpoint}`);
  }

  // POST với FormData (cho upload file)
  postFormData<T>(endpoint: string, formData: FormData): Observable<T> {
    return this.http.post<T>(`${this.API_URL}/${endpoint}`, formData);
  }

  // ========== LEGACY METHODS (for backward compatibility) ==========

  login(usernameOrEmail: string, password: string): Observable<any> {
    return this.post('Auth/login', { usernameOrEmail, password });
  }

  register(
    username: string,
    email: string,
    password: string,
    fullName: string,
    phone?: string,
    idCard?: string
  ): Observable<any> {
    return this.post('Auth/register', {
      username,
      email,
      password,
      fullName,
      phone,
      idCard,
    });
  }
}
