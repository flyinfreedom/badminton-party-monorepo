export interface IGetMemberProfileRequest {
  lineUserId: string;
  memberName: string;
  pictureUrl: string;
}

export interface IMemberProfile {
  memberId: string;
  lineUserId: string;
  pictureUrl: string;
  displayName: string;
  recentOpenings: Array<IRecentOpening>;
}

export interface IRecentOpening {
  courtId: string;
  courtName: string;
  location: string;
  openingTime: string;
}

export interface IAvatarResponse {
  avatarUrl: string;
}

export interface IMemberInitResponse {
  profile: IMemberProfile;
  token: string;
}
