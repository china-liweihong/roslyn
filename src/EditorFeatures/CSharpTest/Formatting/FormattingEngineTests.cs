﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editor.Commands;
using Microsoft.CodeAnalysis.Editor.Implementation.Formatting;
using Microsoft.CodeAnalysis.Editor.Options;
using Microsoft.CodeAnalysis.Editor.Shared.Options;
using Microsoft.CodeAnalysis.Editor.UnitTests.Utilities;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.VisualStudio.Text.Operations;
using Moq;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Formatting
{
    public class FormattingEngineTests : FormattingEngineTestBase
    {
        [WpfFact]
        [WorkItem(539682, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539682")]
        [Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatDocumentCommandHandler()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        int x;$$
int y;
    }
}
";

            var expected = @"class Program
{
    static void Main(string[] args)
    {
        int x;$$
        int y;
    }
}
";

            await AssertFormatWithViewAsync(expected, code);
        }

        [WpfFact]
        [WorkItem(539682, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539682")]
        [Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatDocumentPasteCommandHandler()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        int x;$$
int y;
    }
}
";

            var expected = @"class Program
{
    static void Main(string[] args)
    {
        int x;$$
        int y;
    }
}
";

            await AssertFormatWithPasteOrReturnAsync(expected, code, allowDocumentChanges: true);
        }

        [WpfFact]
        [WorkItem(547261, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/547261")]
        [Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatDocumentReadOnlyWorkspacePasteCommandHandler()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        int x;$$
int y;
    }
}
";

            var expected = @"class Program
{
    static void Main(string[] args)
    {
        int x;$$
int y;
    }
}
";

            await AssertFormatWithPasteOrReturnAsync(expected, code, allowDocumentChanges: false);
        }

        [WpfFact]
        [WorkItem(912965, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/912965")]
        [Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatUsingStatementOnReturn()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        using (null)
                using (null)$$
    }
}
";

            var expected = @"class Program
{
    static void Main(string[] args)
    {
        using (null)
        using (null)$$
    }
}
";

            await AssertFormatWithPasteOrReturnAsync(expected, code, allowDocumentChanges: true, isPaste: false);
        }

        [WpfFact]
        [WorkItem(912965, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/912965")]
        [Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatNotUsingStatementOnReturn()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        using (null)
                for (;;)$$
    }
}
";

            var expected = @"class Program
{
    static void Main(string[] args)
    {
        using (null)
                for (;;)$$
    }
}
";

            await AssertFormatWithPasteOrReturnAsync(expected, code, allowDocumentChanges: true, isPaste: false);
        }

        [WorkItem(977133, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/977133")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatRangeOrFormatTokenOnOpenBraceOnSameLine()
        {
            var code = @"class C
{
    public void M()
    {
        if (true)        {$$
    }
}";
            var expected = @"class C
{
    public void M()
    {
        if (true)        {
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(14491, "https://github.com/dotnet/roslyn/pull/14491")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatRangeButFormatTokenOnOpenBraceOnNextLine()
        {
            var code = @"class C
{
    public void M()
    {
        if (true)
            {$$
    }
}";
            var expected = @"class C
{
    public void M()
    {
        if (true)
        {
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(1007071, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1007071")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatPragmaWarningInbetweenDelegateDeclarationStatement()
        {
            var code = @"using System;

class Program
{
    static void Main(string[] args)
    {
        Func <bool> a = delegate ()
#pragma warning disable CA0001
        {
            return true;
        };$$
    }
}";
            var expected = @"using System;

class Program
{
    static void Main(string[] args)
    {
        Func<bool> a = delegate ()
#pragma warning disable CA0001
        {
            return true;
        };
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(771761, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/771761")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatHashRegion()
        {
            var code = @"using System;

class Program
{
    static void Main(string[] args)
    {
#region$$
    }
}";
            var expected = @"using System;

class Program
{
    static void Main(string[] args)
    {
        #region
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(771761, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/771761")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatHashEndRegion()
        {
            var code = @"using System;

class Program
{
    static void Main(string[] args)
    {
        #region
#endregion$$
    }
}";
            var expected = @"using System;

class Program
{
    static void Main(string[] args)
    {
        #region
        #endregion
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(987373, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/987373")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatSpansIndividuallyWithoutCollapsing()
        {
            var code = @"class C
{
    public void M()
    {
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        if(true){}
        [|if(true){}|]
    }
}";
            var expected = @"class C
{
    public void M()
    {
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if(true){}
        if (true) { }
    }
}";
            using (var workspace = await TestWorkspace.CreateCSharpAsync(code))
            {
                var subjectDocument = workspace.Documents.Single();
                var spans = subjectDocument.SelectedSpans;

                var document = workspace.CurrentSolution.Projects.Single().Documents.Single();
                var syntaxRoot = await document.GetSyntaxRootAsync();

                var node = Formatter.Format(syntaxRoot, spans, workspace);
                Assert.Equal(expected, node.ToFullString());
            }
        }

        [WorkItem(987373, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/987373")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatSpansWithCollapsing()
        {
            var code = @"class C
{
    public void M()
    {
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        [|if(true){}|]
        while(true){}
        [|if(true){}|]
    }
}";
            var expected = @"class C
{
    public void M()
    {
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        if (true) { }
        while (true) { }
        if (true) { }
    }
}";
            using (var workspace = await TestWorkspace.CreateCSharpAsync(code))
            {
                var subjectDocument = workspace.Documents.Single();
                var spans = subjectDocument.SelectedSpans;
                workspace.Options = workspace.Options.WithChangedOption(FormattingOptions.AllowDisjointSpanMerging, true);

                var document = workspace.CurrentSolution.Projects.Single().Documents.Single();
                var syntaxRoot = await document.GetSyntaxRootAsync();

                var node = Formatter.Format(syntaxRoot, spans, workspace);
                Assert.Equal(expected, node.ToFullString());
            }
        }

        [WorkItem(1044118, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1044118")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task SemicolonInCommentOnLastLineDoesNotFormat()
        {
            var code = @"using System;

class Program
{
    static void Main(string[] args)
        {
        }
}
// ;$$";

            var expected = @"using System;

class Program
{
    static void Main(string[] args)
        {
        }
}
// ;";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideSingleLineRegularComment_1()
        {
            var code = @"class Program
{
                              //        {$$
                       static void Main(int a, int b)
    {

    }
}";

            var expected = @"class Program
{
                              //        {
                       static void Main(int a, int b)
    {

    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideSingleLineRegularComment_2()
        {
            var code = @"class Program
{
                              //        {$$   
                       static void Main(int a, int b)
    {

    }
}";

            var expected = @"class Program
{
                              //        {   
                       static void Main(int a, int b)
    {

    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideMultiLineRegularComment_1()
        {
            var code = @"class Program
{
    static void Main(int          a/*         {$$       */, int b)
    {

    }
}";

            var expected = @"class Program
{
    static void Main(int          a/*         {       */, int b)
    {

    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideMultiLineRegularComment_2()
        {
            var code = @"class Program
{
    static void Main(int          a/*         {$$
        */, int b)
    {

    }
}";

            var expected = @"class Program
{
    static void Main(int          a/*         {
        */, int b)
    {

    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideMultiLineRegularComment_3()
        {
            var code = @"class Program
{
    static void Main(int          a/*         {$$    
        */, int b)
    {

    }
}";

            var expected = @"class Program
{
    static void Main(int          a/*         {    
        */, int b)
    {

    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideSingleLineDocComment_1()
        {
            var code = @"class Program
{
                              ///        {$$
                       static void Main(int a, int b)
    {

    }
}";

            var expected = @"class Program
{
                              ///        {
                       static void Main(int a, int b)
    {

    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideSingleLineDocComment_2()
        {
            var code = @"class Program
{
                              ///        {$$   
                       static void Main(int a, int b)
    {

    }
}";

            var expected = @"class Program
{
                              ///        {   
                       static void Main(int a, int b)
    {

    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideMultiLineDocComment_1()
        {
            var code = @"class Program
{
                              /**        {$$   **/
                       static void Main(int a, int b)
    {

    }
}";

            var expected = @"class Program
{
                              /**        {   **/
                       static void Main(int a, int b)
    {

    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideMultiLineDocComment_2()
        {
            var code = @"class Program
{
                              /**        {$$   
                **/
                       static void Main(int a, int b)
    {

    }
}";

            var expected = @"class Program
{
                              /**        {   
                **/
                       static void Main(int a, int b)
    {

    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideMultiLineDocComment_3()
        {
            var code = @"class Program
{
                              /**        {$$
                **/
                       static void Main(int a, int b)
    {

    }
}";

            var expected = @"class Program
{
                              /**        {
                **/
                       static void Main(int a, int b)
    {

    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideInactiveCode()
        {
            var code = @"class Program
{
                                        #if false
                    {$$
            #endif

    static void Main(string[] args)
    {

    }
}";

            var expected = @"class Program
{
                                        #if false
                    {
            #endif

    static void Main(string[] args)
    {

    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideStringLiteral()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var asdas =     ""{$$""        ;
    }
}";

            var expected = @"class Program
{
    static void Main(string[] args)
    {
        var asdas =     ""{""        ;
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideCharLiteral()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var asdas =     '{$$'        ;
    }
}";

            var expected = @"class Program
{
    static void Main(string[] args)
    {
        var asdas =     '{'        ;
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(449, "https://github.com/dotnet/roslyn/issues/449")]
        [WorkItem(1077103, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1077103")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task NoFormattingInsideCommentsOfPreprocessorDirectives()
        {
            var code = @"class Program
{
       #region
        #endregion // a/*{$$*/    
        static void Main(string[] args)
    {
        
    }
}";

            var expected = @"class Program
{
       #region
        #endregion // a/*{*/    
        static void Main(string[] args)
    {
        
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(464, "https://github.com/dotnet/roslyn/issues/464")]
        [WorkItem(908729, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/908729")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task ColonInSwitchCase()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        int f = 0;
        switch(f)
        {
                 case     1     :$$    break;
        }
    }
}";

            var expected = @"class Program
{
    static void Main(string[] args)
    {
        int f = 0;
        switch(f)
        {
            case 1:    break;
        }
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(464, "https://github.com/dotnet/roslyn/issues/464")]
        [WorkItem(908729, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/908729")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task ColonInDefaultSwitchCase()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        int f = 0;
        switch(f)
        {
            case 1:    break;
                    default    :$$     break;
        }
    }
}";

            var expected = @"class Program
{
    static void Main(string[] args)
    {
        int f = 0;
        switch(f)
        {
            case 1:    break;
            default:     break;
        }
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(9097, "https://github.com/dotnet/roslyn/issues/9097")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task ColonInPatternSwitchCase01()
        {
            var code = @"class Program
{
    static void Main()
    {
        switch(f)
        {
                          case  int  i            :$$    break;
        }
    }
}";

            var expected = @"class Program
{
    static void Main()
    {
        switch(f)
        {
            case int i:    break;
        }
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(464, "https://github.com/dotnet/roslyn/issues/464")]
        [WorkItem(908729, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/908729")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task ColonInLabeledStatement()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
            label1   :$$   int s = 0;
    }
}";

            var expected = @"class Program
{
    static void Main(string[] args)
    {
            label1:   int s = 0;
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(464, "https://github.com/dotnet/roslyn/issues/464")]
        [WorkItem(908729, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/908729")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatColonInTargetAttribute()
        {
            var code = @"using System;
[method    :$$    C]
class C : Attribute
{
}";

            var expected = @"using System;
[method    :    C]
class C : Attribute
{
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(464, "https://github.com/dotnet/roslyn/issues/464")]
        [WorkItem(908729, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/908729")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatColonInBaseList()
        {
            var code = @"class C   :$$   Attribute
{
}";

            var expected = @"class C   :   Attribute
{
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(464, "https://github.com/dotnet/roslyn/issues/464")]
        [WorkItem(908729, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/908729")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatColonInThisConstructor()
        {
            var code = @"class Foo
{
    Foo(int s)   :$$   this()
    {
    }

    Foo()
    {
    }
}";

            var expected = @"class Foo
{
    Foo(int s)   :   this()
    {
    }

    Foo()
    {
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(464, "https://github.com/dotnet/roslyn/issues/464")]
        [WorkItem(908729, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/908729")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatColonInConditionalOperator()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        var vari = foo()     ?    true  :$$  false;
    }
}";

            var expected = @"class Program
{
    static void Main(string[] args)
    {
        var vari = foo()     ?    true  :  false;
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(464, "https://github.com/dotnet/roslyn/issues/464")]
        [WorkItem(908729, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/908729")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatColonInArgument()
        {
            var code = @"class Program
{
    static void Main(string[] args)
    {
        Main(args  :$$  args);
    }
}";

            var expected = @"class Program
{
    static void Main(string[] args)
    {
        Main(args  :  args);
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(464, "https://github.com/dotnet/roslyn/issues/464")]
        [WorkItem(908729, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/908729")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatColonInTypeParameter()
        {
            var code = @"class Program<T>
{
    class C1<U>
        where   T  :$$  U
    {

    }
}";

            var expected = @"class Program<T>
{
    class C1<U>
        where   T  :  U
    {

    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(2224, "https://github.com/dotnet/roslyn/issues/2224")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DontSmartFormatBracesOnSmartIndentNone()
        {
            var code = @"class Program<T>
{
    class C1<U>
{$$
}";

            var expected = @"class Program<T>
{
    class C1<U>
{
}";
            var optionSet = new Dictionary<OptionKey, object>
                            {
                                { new OptionKey(FormattingOptions.SmartIndent, LanguageNames.CSharp), FormattingOptions.IndentStyle.None }
                            };
            await AssertFormatAfterTypeCharAsync(code, expected, optionSet);
        }

        [WpfFact]
        [Trait(Traits.Feature, Traits.Features.SmartTokenFormatting)]
        public async Task StillAutoIndentCloseBraceWhenFormatOnCloseBraceIsOff()
        {
            var code = @"namespace N
{
    class C
    {
             // improperly indented code
             int x = 10;
        }$$
}
";

            var expected = @"namespace N
{
    class C
    {
             // improperly indented code
             int x = 10;
    }
}
";

            var optionSet = new Dictionary<OptionKey, object>
            {
                    { new OptionKey(FeatureOnOffOptions.AutoFormattingOnCloseBrace, LanguageNames.CSharp), false }
            };

            await AssertFormatAfterTypeCharAsync(code, expected, optionSet);
        }

        [WpfFact, WorkItem(4435, "https://github.com/dotnet/roslyn/issues/4435")]
        [Trait(Traits.Feature, Traits.Features.SmartTokenFormatting)]
        public async Task OpenCurlyNotFormattedIfNotAtStartOfLine()
        {
            var code = 
@"
class C
{
    public  int     P   {$$
}
";

            var expected =
@"
class C
{
    public  int     P   {
}
";

            var optionSet = new Dictionary<OptionKey, object>
            {
                { new OptionKey(BraceCompletionOptions.EnableBraceCompletion, LanguageNames.CSharp), false }
            };

            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WpfFact, WorkItem(4435, "https://github.com/dotnet/roslyn/issues/4435")]
        [Trait(Traits.Feature, Traits.Features.SmartTokenFormatting)]
        public async Task OpenCurlyFormattedIfAtStartOfLine()
        {
            var code =
@"
class C
{
    public  int     P
        {$$
}
";

            var expected =
@"
class C
{
    public  int     P
    {
}
";

            var optionSet = new Dictionary<OptionKey, object>
            {
                { new OptionKey(BraceCompletionOptions.EnableBraceCompletion, LanguageNames.CSharp), false }
            };

            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatIncompleteBlockOnSingleLineIfNotTypingCloseCurly1()
        {
            var code = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property
        {
            get { return true;$$
    }
}";
            var expected = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property
        {
            get { return true;
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatIncompleteBlockOnSingleLineIfNotTypingCloseCurly2()
        {
            var code = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property { get { return true;$$
    }
}";
            var expected = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property { get { return true;
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatIncompleteBlockOnSingleLineIfNotTypingCloseCurly3()
        {
            var code = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property { get;$$
    }
}";
            var expected = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property { get;
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatCompleteBlockOnSingleLineIfTypingCloseCurly1()
        {
            var code = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property
        {
            get { return true; }$$
}";
            var expected = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property
        {
            get { return true; }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatCompleteBlockOnSingleLineIfTypingCloseCurly2()
        {
            var code = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property { get { return true; }$$
}";
            var expected = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property { get { return true; }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatIncompleteBlockOnMultipleLinesIfTypingCloseCurly1()
        {
            var code = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property
        {
            get { return true;
    }$$
}";
            var expected = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property
        {
            get
            {
                return true;
            }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatIncompleteBlockOnMultipleLinesIfTypingCloseCurly2()
        {
            var code = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property
        {
            get { return true;
    }
}$$";
            var expected = @"namespace ConsoleApplication1
{
    class Program
    {
        static bool Property
        {
            get
            {
                return true;
            }
        }";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task DoNotFormatCompleteBlockOnSingleLineIfTypingSemicolon()
        {
            var code =
@"public class Class1
{
    void M()
    {
        try { }
        catch { return;$$
        x.ToString();
    }
}";
            var expected = 
@"public class Class1
{
    void M()
    {
        try { }
        catch { return;
        x.ToString();
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatCompleteBlockOnSingleLineIfTypingCloseCurlyOnLaterLine()
        {
            var code =
@"public class Class1
{
    void M()
    {
        try { }
        catch { return;
        x.ToString();
        }$$
    }
}";
            var expected =
@"public class Class1
{
    void M()
    {
        try { }
        catch
        {
            return;
            x.ToString();
        }
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(7900, "https://github.com/dotnet/roslyn/issues/7900")]
        [Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatLockStatementWithEmbeddedStatementOnSemicolonDifferentLine()
        {
            var code = @"class C
{
    private object _l = new object();
    public void M()
    {
        lock (_l)
                       Console.WriteLine(""d"");$$
    }
}";
            var expected = @"class C
{
    private object _l = new object();
    public void M()
    {
        lock (_l)
            Console.WriteLine(""d"");
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(7900, "https://github.com/dotnet/roslyn/issues/7900")]
        [Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatLockStatementWithEmbeddedStatementOnSemicolonSameLine()
        {
            var code = @"class C
{
    private object _l = new object();
    public void M()
    {
        lock (_l)      Console.WriteLine(""d"");$$
    }
}";
            var expected = @"class C
{
    private object _l = new object();
    public void M()
    {
        lock (_l) Console.WriteLine(""d"");
    }
}";
            await AssertFormatAfterTypeCharAsync(code, expected);
        }

        [WorkItem(11642, "https://github.com/dotnet/roslyn/issues/11642")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Formatting)]
        public async Task FormatArbitraryNodeParenthesizedLambdaExpression()
        {
            // code equivalent to an expression synthesized like so:
            // ParenthesizedExpression(ParenthesizedLambdaExpression(ParameterList(), Block()))
            var code = @"(()=>{})";
            var node = SyntaxFactory.ParseExpression(code);
            var expected = @"(() => { })";
            await AssertFormatOnArbitraryNodeAsync(node, expected);
        }

        private static async Task AssertFormatAfterTypeCharAsync(string code, string expected, Dictionary<OptionKey, object> changedOptionSet = null)
        {
            using (var workspace = await TestWorkspace.CreateCSharpAsync(code))
            {
                if (changedOptionSet != null)
                {
                    var options = workspace.Options;
                    foreach (var entry in changedOptionSet)
                    {
                        options = options.WithChangedOption(entry.Key, entry.Value);
                    }

                    workspace.Options = options;
                }

                var subjectDocument = workspace.Documents.Single();

                var textUndoHistory = new Mock<ITextUndoHistoryRegistry>();
                var editorOperationsFactory = new Mock<IEditorOperationsFactoryService>();
                var editorOperations = new Mock<IEditorOperations>();
                editorOperationsFactory.Setup(x => x.GetEditorOperations(subjectDocument.GetTextView())).Returns(editorOperations.Object);

                var commandHandler = new FormatCommandHandler(TestWaitIndicator.Default, textUndoHistory.Object, editorOperationsFactory.Object);
                var typedChar = subjectDocument.GetTextBuffer().CurrentSnapshot.GetText(subjectDocument.CursorPosition.Value - 1, 1);
                commandHandler.ExecuteCommand(new TypeCharCommandArgs(subjectDocument.GetTextView(), subjectDocument.TextBuffer, typedChar[0]), () => { });

                var newSnapshot = subjectDocument.TextBuffer.CurrentSnapshot;

                Assert.Equal(expected, newSnapshot.GetText());
            }
        }
    }
}
