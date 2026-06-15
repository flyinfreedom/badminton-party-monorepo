export class StringExtensions {
  static padLeft(input: string | number, length: number, paddingChar: string = '0'): string {
    const inputStr: string = input.toString();
    if (inputStr.length >= length) {
      return inputStr;
    }

    const padding = Array(length - inputStr.length + 1).join(paddingChar);
    return padding + input;
  }
}
