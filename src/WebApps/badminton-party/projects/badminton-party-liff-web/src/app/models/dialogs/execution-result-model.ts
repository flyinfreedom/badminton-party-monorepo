import { ExecutionDialogType } from "../../enums";

export interface IExecutionResult {
  title: string | null;
  message: string;
  type: ExecutionDialogType;
}
