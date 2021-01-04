---
uid: Guides.Advanced.Validation
title: Validation with Venflow
---

# Validation with Venflow

Venflow performs a lot of validation under the hood, to perform the best possible UX. However this comes at a performance trade-off. Therefor Venflow only performs these validation, if you are using Venflow in a `DEBUG` build. If you would compile your assembly to `RELEASE` these validations wouldn't be performed. In order to manually override this behaviour you can configure this setting with the static [`VenflowConfiguration`](xref:Venflow.VenflowConfiguration) class.

In the below example we would tell Venflow, to always use 'Deep Validation', no matter the configuration. You should place this at very beginning of your program, however you can change this value whenever you want.

```cs
VenflowConfiguration.UseDeepValidation(true);
```

