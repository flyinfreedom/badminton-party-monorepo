import { ConsumptionPatterns, GroupStatus, LevelGroup } from "../enums";

export interface IGroupFormRequest {
  groupName: string;
  startTime: string;
  playTime: number;
  courtId: string;
  courtName: string;
  location: string;
  consumptionPatterns: ConsumptionPatterns;
  amount: number;
  minPeople: number;
  maxPeople: number;
  alternatePeople: number;
  levelGroup: LevelGroup;
  otherInfo: string;
}

export interface IGroup {
  groupId: string;
  groupName: string;
  groupStatus: GroupStatus;
  avatar: string;
  startTime: string;
  endTime: string;
  playTime: number;
  courtId: string;
  courtName: string;
  location: string;
  consumptionPatterns: ConsumptionPatterns;
  amount: number;
  minPeople: number;
  maxPeople: number;
  alternatePeople: number;
  levelGroup: LevelGroup;
  otherInfo: string;
  memberId: string;
  memberName: string;
  isPrivate: boolean;
  createTime: string;
  updateTime: string;
  joinedMembers: Array<IGroupMember>
}

export interface IGroupMember {
  memberId: string;
  lineUserId: string;
  displayName: string;
  pictureUrl: string;
  joinTime: string;
}

export interface IGetMyGroupResponse {
  createdGroups: Array<IGroup>;
  joinedGroups: Array<IGroup>;
}
