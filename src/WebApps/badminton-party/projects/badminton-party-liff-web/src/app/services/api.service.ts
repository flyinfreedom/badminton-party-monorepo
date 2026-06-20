import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { IAvatarResponse, IGetMemberProfileRequest, IMemberProfile } from '../models/api-model';
import { ICourt, IGetMyGroupResponse, IGroup, IGroupFormRequest, IGroupMember } from '../models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  constructor(private httpClient: HttpClient) {}

  public getMemberProfile(request: IGetMemberProfileRequest): Observable<IMemberProfile> {
    return this.httpClient.post<IMemberProfile>('api/member/init', request);
  }

  public getGroupById(groupId: string): Observable<IGroup> {
    return this.httpClient.get<IGroup>(`api/group/${groupId}`);
  }

  public getMyCurrentGroup(): Observable<IGetMyGroupResponse> {
    return this.httpClient.get<IGetMyGroupResponse>('api/group');
  }

  public getMyHistoryGroup(startTimeYearMonth: number): Observable<IGetMyGroupResponse> {
    return this.httpClient.get<IGetMyGroupResponse>(`api/group/history/${startTimeYearMonth}`);
  }

  public joinGroup(groupId: string): Observable<Array<IGroupMember>> {
    return this.httpClient.post<Array<IGroupMember>>(`api/group/${groupId}/join`, null);
  }

  public minusOneMember(groupId: string): Observable<Array<IGroupMember>> {
    return this.httpClient.post<Array<IGroupMember>>(`api/group/${groupId}/minus`, null);
  }

  public leaveGroup(groupId: string): Observable<Array<IGroupMember>> {
    return this.httpClient.post<Array<IGroupMember>>(`api/group/${groupId}/leave`, null);
  }

  public createGroup(request: IGroupFormRequest): Observable<string> {
    return this.httpClient.post<string>('api/group', request);
  }

  public updateGroup(groupId: string, request: IGroupFormRequest): Observable<IGroupFormRequest> {
    return this.httpClient.put<IGroupFormRequest>(`api/group/${groupId}`, request);
  }

  public closeGroup(groupId: string): Observable<boolean> {
    return this.httpClient.post<boolean>(`api/group/${groupId}/close`, null);
  }

  public removeGroupMember(groupId: string, memberId: string): Observable<Array<IGroupMember>> {
    return this.httpClient.post<Array<IGroupMember>>(`api/group/remove_group_member/${groupId}/${memberId}`, null);
  }

  public getCourts(): Observable<ICourt[]> {
    return this.httpClient.get<ICourt[]>(`api/court`);
  }

  public getCourtById(courtId: string): Observable<ICourt> {
    return this.httpClient.get<ICourt>(`api/court/${courtId}`);
  }

  public getGroupsByCourtId(courtId: string): Observable<IGroup[]> {
    return this.httpClient.get<IGroup[]>(`api/court/groups/${courtId}`);
  }

  public updateUserName(name: string): Observable<string> {
    return this.httpClient.put<string>('api/member/name', { name });
  }

  public uploadAvatar(file: FormData): Observable<IAvatarResponse> {
    return this.httpClient.post<IAvatarResponse>('api/member/avatar', file);
  }
}
