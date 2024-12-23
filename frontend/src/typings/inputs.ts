export type InputChanged<T = unknown> = {
  name: string;
  value: T;
};

export type InputOnChange<T> = (change: InputChanged<T>) => void;

export interface CheckInputChanged extends InputChanged<boolean> {
  shiftKey: boolean;
}
