import { useStore } from '@tanstack/react-form'
import { Button, Text, TextInput, Textarea, Select as MantineSelect } from '@mantine/core'

import { useFieldContext, useFormContext } from '@/hooks/demo.form-context'

export function SubscribeButton({ label }: { label: string }) {
  const form = useFormContext()
  return (
    <form.Subscribe selector={(state) => state.isSubmitting}>
      {(isSubmitting) => (
        <Button type="submit" disabled={isSubmitting} loading={isSubmitting}>
          {label}
        </Button>
      )}
    </form.Subscribe>
  )
}

function ErrorMessages({
  errors,
}: {
  errors: Array<string | { message: string }>
}) {
  return (
    <>
      {errors.map((error) => (
        <Text
          key={typeof error === 'string' ? error : error.message}
          c="red"
          size="sm"
          fw={700}
          mt="xs"
        >
          {typeof error === 'string' ? error : error.message}
        </Text>
      ))}
    </>
  )
}

export function TextField({
  label,
  placeholder,
}: {
  label: string
  placeholder?: string
}) {
  const field = useFieldContext<string>()
  const errors = useStore(field.store, (state) => state.meta.errors)

  return (
    <TextInput
      label={label}
      placeholder={placeholder}
      value={field.state.value}
      onBlur={field.handleBlur}
      onChange={(e) => field.handleChange(e.target.value)}
      error={field.state.meta.isTouched && errors.length > 0 ? errors[0] : undefined}
      size="md"
      styles={{
        label: {
          fontSize: '1.25rem',
          fontWeight: 700,
        },
      }}
    />
  )
}

export function TextArea({
  label,
  rows = 3,
}: {
  label: string
  rows?: number
}) {
  const field = useFieldContext<string>()
  const errors = useStore(field.store, (state) => state.meta.errors)

  return (
    <Textarea
      label={label}
      value={field.state.value}
      onBlur={field.handleBlur}
      rows={rows}
      onChange={(e) => field.handleChange(e.target.value)}
      error={field.state.meta.isTouched && errors.length > 0 ? errors[0] : undefined}
      size="md"
      styles={{
        label: {
          fontSize: '1.25rem',
          fontWeight: 700,
        },
      }}
    />
  )
}

export function Select({
  label,
  values,
  placeholder,
}: {
  label: string
  values: Array<{ label: string; value: string }>
  placeholder?: string
}) {
  const field = useFieldContext<string>()
  const errors = useStore(field.store, (state) => state.meta.errors)

  return (
    <MantineSelect
      label={label}
      placeholder={placeholder}
      value={field.state.value}
      onBlur={field.handleBlur}
      onChange={(value) => field.handleChange(value || '')}
      data={values.map((v) => ({ label: v.label, value: v.value }))}
      error={field.state.meta.isTouched && errors.length > 0 ? errors[0] : undefined}
      size="md"
      styles={{
        label: {
          fontSize: '1.25rem',
          fontWeight: 700,
        },
      }}
    />
  )
}
