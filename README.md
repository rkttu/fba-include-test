# fba-include-test

.NET 10 SDK **10.0.300** 릴리스에서 새롭게 지원되는 **`#:include`** 디렉티브를 활용해, 파일 기반 C# 앱(file-based app)을 여러 파일로 분리해보는 최소 예제입니다. Native AOT로 빌드되며, 포인터 산술과 `NativeMemory.Alloc`/`Free` 를 사용해 피보나치 수열 10개를 출력합니다.

## 핵심 포인트: `#:include`

기존의 파일 기반 앱은 단일 `.cs` 파일 안에 모든 코드와 `#:` 디렉티브를 함께 작성해야 했습니다. .NET 10 SDK 10.0.300 부터는 `#:include` 로 **다른 `.cs` 파일을 끌어와** 코드와 빌드 구성을 함께 분리할 수 있습니다.

[main.cs](main.cs#L2) 의 한 줄이 전부입니다:

```csharp
#:include ./include/native.cs
```

이 디렉티브 하나로 [include/native.cs](include/native.cs) 의 다음 내용이 모두 들어옵니다:

- `#:property` 빌드 속성 (AOT, unsafe 허용 등)
- `global using` 선언 (`System.Console`, `NativeMemory` 등 정적 멤버 포함)

덕분에 [main.cs](main.cs) 에는 알고리즘 본문만 남게 됩니다.

## 전제 조건: 전이적 디렉티브 활성화

`#:include` 로 가져온 파일의 `#:property` 가 **포함하는 쪽에도 적용**되도록 하려면, **전이적(transitive) 디렉티브 처리**를 켜야 합니다. 이는 현재 실험적 기능이며 [include/native.cs:1](include/native.cs#L1) 에서 켭니다:

```csharp
#:property ExperimentalFileBasedProgramEnableTransitiveDirectives=True
```

이 속성이 꺼져 있으면 included 파일의 `#:property PublishAot=true` 같은 설정이 무시되고, 컴파일 단계에서 `AllowUnsafeBlocks` 미설정으로 실패합니다.

다만 이 Opt-In 설정은 2026년 5월 현재 전이적 디렉티브 처리 기능이 프리뷰로 출시되었기 때문에 필요한 설정으로 추후에는 별도로 켜는 설정을 지정하지 않아도 되도록 변경될 수 있습니다.

## 프로젝트 구조

```text
.
├─ main.cs                # 진입점 (피보나치 unsafe 루프)
├─ include/
│  └─ native.cs           # 공유 빌드 속성 + global using
└─ artifacts/app/         # publish 결과물 (gitignored)
```

### [main.cs](main.cs)

- `#!/usr/bin/env dotnet` 셔뱅 — 유닉스에서는 `chmod +x main.cs` 후 직접 실행 가능
- `#:include ./include/native.cs` 한 줄로 빌드 속성과 using 을 모두 끌어옴
- `unsafe { ... }` 블록 안에서 `long*` 포인터 산술로 피보나치 10개 계산

### [include/native.cs](include/native.cs)

| 디렉티브 | 역할 |
| --- | --- |
| `ExperimentalFileBasedProgramEnableTransitiveDirectives=True` | included 파일의 `#:` 디렉티브를 호출 측에도 적용 |
| `AllowUnsafeBlocks=true` | `unsafe` 코드 컴파일 허용 |
| `PublishAot=true` | Native AOT 로 publish |
| `ImplicitUsings=disable` | SDK 기본 implicit using 비활성화 (이 파일에서 직접 명시) |
| `global using static System.Console` | `WriteLine` 을 한정자 없이 호출 |
| `global using static System.Runtime.InteropServices.NativeMemory` | `Alloc`/`Free` 를 한정자 없이 호출 |

## 실행 방법

### 사전 요구 사항

- **.NET 10 SDK 10.0.300 이상** (`dotnet --version` 으로 확인)
- Native AOT publish 시 플랫폼별 C 컴파일러 툴체인 (Windows: VS Build Tools / MSVC, Linux: clang, macOS: Xcode CLT)

### 그냥 실행

**Windows를 포함한 대부분의 OS의 경우:**

```powershell
dotnet run main.cs
```

**Linux, macOS의 경우:**

```bash
chmod +x ./main.cs
./main.cs
```

**기대 출력:**

```text
0
1
1
2
3
5
8
13
21
34
```

### Native AOT 로 publish

```powershell
dotnet publish main.cs -o artifacts/app
./artifacts/app/app.exe
```

`PublishAot=true` 가 included 파일에서 전이적으로 적용되므로 별도 옵션 없이 네이티브 실행 파일이 만들어집니다.

## 참고

- [File-based apps — .NET 공식 문서](https://learn.microsoft.com/dotnet/core/sdk/file-based-apps)
- [What's new in the SDK and tooling for .NET 10](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk#file-based-apps-enhancements)
- [C# preprocessor directives — `#:` 디렉티브](https://learn.microsoft.com/dotnet/csharp/language-reference/preprocessor-directives)

## 라이선스

이 코드 샘플은 [MIT 라이선스](LICENSE) 로 배포됩니다.
