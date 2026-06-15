'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import type { ReactNode } from 'react';
import React, { createContext, useContext, useEffect, useRef, useTransition } from 'react';
import type { DefaultValues, FieldErrors, FieldValues, FormState, Path, Resolver } from 'react-hook-form';
import {
  useForm,
  useFormState,
  type Control,
  type FieldErrorsImpl,
  type FieldNamesMarkedBoolean,
  type SubmitErrorHandler,
  type UseFormGetValues,
  type UseFormRegister,
  type UseFormReset,
  type UseFormSetValue,
  type UseFormTrigger,
  type UseFormWatch
} from 'react-hook-form';
import type { ZodType } from 'zod';

export type FormMode = 'create' | 'update';

export type FormContextProps<T extends FieldValues = FieldValues> = {
  onSubmit: (values: T) => void | Promise<unknown>;
  onError: (errors: FieldErrors<T>) => void;
  setValue: UseFormSetValue<T>;
  reset: UseFormReset<T>;
  getValues: UseFormGetValues<T>;
  formState?: FormState<T>;
  validationErrors: Partial<FieldErrorsImpl<T>> | undefined;
  control?: Control<T>;
  watch: UseFormWatch<T>;
  submitting: boolean;
  isDirty: boolean;
  isValid: boolean;
  dirtyFields: Partial<FieldNamesMarkedBoolean<T>>;
  readOnly?: boolean;
  trigger: UseFormTrigger<T>;
  register: UseFormRegister<T>;
};

const FormContext = createContext<FormContextProps<FieldValues> | null>(null);

export interface ChangeFieldDelegate<T extends FieldValues = FieldValues> {
  fieldName: Path<T>;
  delegate: (fieldValue: unknown, setValue: UseFormSetValue<T>, watch?: UseFormWatch<T>) => void;
}

interface ChangeFieldHandlerProps<T extends FieldValues> {
  handler: ChangeFieldDelegate<T>;
  children: ReactNode;
}

function ChangeFieldHandler<T extends FieldValues>({ handler, children }: ChangeFieldHandlerProps<T>) {
  const { watch, setValue, dirtyFields } = useFormContext<T>();
  const currentValue = watch(handler.fieldName);

  useEffect(() => {
    const key = handler.fieldName as string;
    if ((dirtyFields as Partial<Record<string, unknown>>)[key]) {
      handler.delegate(currentValue, setValue, watch);
    }
  }, [handler, currentValue, dirtyFields, setValue, watch]);

  return <>{children}</>;
}

interface ChangeFieldHandlersProps<T extends FieldValues> {
  handlers: ChangeFieldDelegate<T>[] | undefined;
  children: ReactNode;
}

function ChangeFieldHandlers<T extends FieldValues>({ handlers, children }: ChangeFieldHandlersProps<T>) {
  if (!handlers?.length) {
    return <>{children}</>;
  }

  let current: ReactNode = children;
  for (let i = handlers.length - 1; i >= 0; i--) {
    const h = handlers[i];
    current = (
      <ChangeFieldHandler key={`${String(h.fieldName)}-${i}`} handler={h}>
        {current}
      </ChangeFieldHandler>
    );
  }
  return <>{current}</>;
}

export interface FormProviderProps<T extends FieldValues = FieldValues> {
  children: ReactNode;
  validationSchema: ZodType<T>;
  defaultValues: DefaultValues<T>;
  onSubmit: (values: T) => void | Promise<unknown>;
  onError?: SubmitErrorHandler<T>;
  onChangeField?: ChangeFieldDelegate<T>[];
  readOnly?: boolean;
}

export function FormProvider<T extends FieldValues = FieldValues>({
  children,
  validationSchema,
  defaultValues,
  onSubmit,
  onError,
  onChangeField,
  readOnly = false
}: FormProviderProps<T>) {
  const [isSubmitPending, startSubmitTransition] = useTransition();
  const wasPendingRef = useRef(false);

  const {
    handleSubmit,
    setValue,
    getValues,
    control,
    reset,
    formState,
    watch,
    trigger,
    register,
    formState: { isDirty, isValid, dirtyFields }
  } = useForm<T>({
    resolver: zodResolver(validationSchema as never) as Resolver<T>,
    defaultValues: defaultValues as DefaultValues<T>
  });

  useEffect(() => {
    if (wasPendingRef.current && !isSubmitPending) {
      reset(getValues());
    }
    wasPendingRef.current = isSubmitPending;
  }, [isSubmitPending, reset, getValues]);

  const { errors } = useFormState({ control });
  const validationErrors = Object.keys(errors ?? {}).length ? errors : undefined;

  const formSubmit = (values: T) => {
    return new Promise<void>((resolve, reject) => {
      startSubmitTransition(async () => {
        try {
          await onSubmit(values);
          resolve();
        } catch (err) {
          reject(err);
        }
      });
    });
  };

  const htmlSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    event.stopPropagation();
    return handleSubmit(formSubmit, onError)(event);
  };

  const setValueCustom: UseFormSetValue<T> = (name, value, options) => setValue(name, value, options);

  const value: FormContextProps<T> = {
    readOnly,
    onSubmit,
    onError: onError ?? (() => undefined),
    setValue: setValueCustom,
    control,
    reset,
    formState,
    getValues,
    validationErrors,
    watch,
    submitting: isSubmitPending,
    isDirty,
    isValid,
    dirtyFields,
    trigger,
    register
  };

  return (
    <FormContext.Provider value={value as FormContextProps<FieldValues>}>
      <ChangeFieldHandlers handlers={onChangeField}>
        <form className="formContext" onSubmit={htmlSubmit}>
          {children}
        </form>
      </ChangeFieldHandlers>
    </FormContext.Provider>
  );
}

export function useFormContext<T extends FieldValues = FieldValues>() {
  const ctx = useContext(FormContext) as FormContextProps<T> | null;
  if (!ctx) {
    throw new Error('useFormContext deve ser usado dentro de FormProvider.');
  }
  return ctx;
}
